# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

`article-news-raw` is the **article-news-raw** microservice in the TTM (Timing The Market) polyrepo — see `../CLAUDE.md` for how it fits with the other services. It's a raw-ingestion service: on a Hangfire schedule it pulls financial news article URLs from external news APIs (Finnhub, AlphaVantage) and persists them to Postgres, ready for a downstream service to fetch/parse full article content.

## Solution Structure

Three projects (Clean-Architecture-shaped) plus the shared library, all in `article_news_raw.sln`:

- **`article_news_raw`** — ASP.NET Core Web API host. `Program.cs` wires up Kestrel (port 5007), Hangfire, MassTransit, Serilog/OpenTelemetry, and DI (`DiContainer.cs`). Contains `Controllers/`, `Scheduler/` (Hangfire job setup), `Triggers/` (MassTransit consumers), `Filters/` (exception-to-event middleware).
- **`article_news_raw.Domain`** — business logic. `Handlers/` orchestrate fetching; `Handlers/FetchNews/` has one handler per news source implementing `IFetchNewsUrlsHandler`; `Utils/` has fuzzy/cosine text-similarity helpers (`TextHelper`, `MathUtils`) used for de-duplicating/matching articles.
- **`article_news_raw.DataAccess`** — EF Core `DbContext`, repositories, external API clients (`Services/FinnhubApiNewsService`, `Services/AlphaVantageApiNewsService`, `Services/SitemapService`), and API/DB models. References `TTM.Shared` directly.

## Common Development Commands

Run from this directory:

```bash
# Build
dotnet build article_news_raw.sln

# Run the API
dotnet run --project article_news_raw/article_news_raw.csproj
```

There is no `.Tests` project in this service currently.

```bash
# Docker (per README.md)
docker build --tag 'article-news-raw' .
docker run --detach 'article-news-raw' --memory=512m
```

Note the Dockerfile's build context is the monorepo root (it `COPY`s `article-news-raw/...` and `TTM.Shared/...` paths), not this directory — build from `ttm-system/`.

### Required environment variables
- `POSTGRESSQL_CONN` — Postgres connection string (throws on startup if missing)
- `FINNHUB_API_KEY` — required by `FinnhubApiNewsService` (only throws if that service is actually resolved/used)
- `ALPHAVANTAGE_API_KEY` — required by `AlphaVantageApiNewsService`
- `INFRA_SERVICE_URL` — central log sink (gRPC), defaults to `http://localhost:4317`
- `OLT_ENDPOINT` — OTLP tracing collector, defaults to `http://localhost:4317`
- `MASTERDATA_URL` — gRPC endpoint for `securities-masterdata`, used by the sector sentiment report to resolve ticker→sector; defaults to `http://localhost:5101`
- `DISCORD_SENTIMENT_ID` / `DISCORD_SENTIMENT_TOKEN` — Discord webhook id/token the sector sentiment report posts to (throws on use if missing; distinct from portfolio's `DISCORD_ID`/`DISCORD_TOKEN`)

## Architecture Notes

### Fetch pipeline
`FetchNewsUrlsHandler` (Domain) fans out to every registered `IFetchNewsUrlsHandler` and runs each one, catching and reporting exceptions per-source via `SendSystemError` rather than failing the whole run:
- `FetchAlphavantageApiUrlsNewsHandler` — pulls from 6 AlphaVantage `NEWS_SENTIMENT` topics (finance, manufacturing, financial_markets, economy_macro, earnings, retail_wholesale), rate-limits itself between calls (throttled to stay under 5 calls/min), de-dupes against existing DB rows before insert, and maps ticker sentiment scores onto `ArticleTickerSentiment`.
- `FetchFinnhubApiUrlNewsHandler` — pulls general-category news from Finnhub, tracks the max article ID seen for incremental fetches. **Currently not registered in DI** (`DiContainer.cs` has it commented out) — only the AlphaVantage handler runs by default.

Only implementations registered in `DiContainer.AddCustomServices` actually run; check that file, not just the `Handlers/FetchNews/` folder, to know what's live.

### Two ways to trigger a fetch
1. **Scheduled**: `SetupHangfireJobs` registers a recurring job (`*/10 * * * *`, UTC) that publishes a `FetchNewesUrlsTriggerEvent` via MassTransit, consumed by `FetchNewsUrlsTrigger` which calls the handler.
2. **On-demand via HTTP**: `ArticleController` exposes `GET /article/trigger-url-fetch?toDate=` (single run) and `GET /article/trigger-url-fetch-range?fromDate=&toDate=` (hourly-chunked backfill, 5 requests in parallel per chunk) — useful for backfilling a date range.

### Sector sentiment report
`SetupHangfireJobs` also registers `"weekly-sector-sentiment-report"` (cron `0 6 * * 6`, UTC — every Saturday 06:00), publishing `GenerateSectorSentimentReportTriggerEvent`, consumed by `SectorSentimentReportTrigger`, which calls `GenerateSectorSentimentReportHandler`. That handler fetches all securities (ticker + sector) from `securities-masterdata` via the `IMasterdataService` gRPC client, queries `IQryArticleNewsSentimentHandler` for per-ticker sentiment over three independent windows (last 7/14/30 days), aggregates per sector (weighted-by-occurrence average, simple average, total occurrences, top 3 by sentiment, top 3 by occurrence count), and posts one Discord message per window via the generic `IDiscordService`/`AddTtmDiscordService` added to `TTM.Shared`. Can be triggered on demand via `GET /article/trigger-sector-sentiment-report`, bypassing Hangfire/MassTransit — useful for testing without waiting for Saturday.

### MassTransit is in-memory only
Unlike the root `CLAUDE.md`'s general description of MassTransit + RabbitMQ, this service currently configures MassTransit with `UsingInMemory` (`DiContainer.ConfigureMasstransit`). The internal Hangfire→trigger event (`FetchNewesUrlsTriggerEvent`) and system-error publishing stay in-process; there's no cross-service event publishing wired up here yet.

### Database
EF Core against Postgres (`ArticleNewsDbContext`), tables `article_url` and `article_ticker_sentiment` (snake_case columns, configured via `OnModelCreating` — no separate FluentMigrator project in this service, unlike other TTM services). `ArticleUrlRepository` uses `IDbContextFactory` to create a fresh, short-lived `DbContext` per operation rather than a scoped injected context.

### Unused/in-progress code
`SitemapService` (fetches and recursively parses XML sitemaps, with gzip/deflate/brotli decompression handling) exists but is not registered in DI or referenced by any handler — treat it as scaffolding for a not-yet-wired sitemap-based ingestion path.
