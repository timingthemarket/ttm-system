# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

`article-news-raw` is the **article-news-raw** microservice in the TTM (Timing The Market) polyrepo — see `../CLAUDE.md` for how it fits with the other services. It's a raw-ingestion service: on a Hangfire schedule it pulls financial news article URLs from external news APIs (Finnhub, AlphaVantage) and persists them to Postgres, ready for a downstream service to fetch/parse full article content.

## Solution Structure

Three projects (Clean-Architecture-shaped) plus the shared library, all in `article_news_raw.sln`:

- **`article_news_raw`** — ASP.NET Core Web API host. `Program.cs` wires up Kestrel (port 5007), Hangfire, MassTransit, Serilog/OpenTelemetry, and DI (`DiContainer.cs`). Contains `Controllers/`, `Scheduler/` (Hangfire job setup), `Triggers/` (MassTransit consumers), `Filters/` (exception-to-event middleware), `Migrations/` (FluentMigrator).
- **`article_news_raw.Domain`** — business logic. `Handlers/` orchestrate fetching; `Handlers/FetchNews/` has one handler per news source implementing `IFetchNewsUrlsHandler`; `Utils/` has fuzzy/cosine text-similarity helpers (`TextHelper`, `MathUtils`) used for de-duplicating/matching articles.
- **`article_news_raw.DataAccess`** — EF Core `DbContext`, repositories, external API clients (`Services/FinnhubApiNewsService`, `Services/AlphaVantageApiNewsService`, `Services/AlphaVantageCommoditiesService`, `Services/SitemapService`), and API/DB models. References `TTM.Shared` directly.

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
- `ALPHAVANTAGE_API_KEY` — required by `AlphaVantageApiNewsService` and `AlphaVantageCommoditiesService`
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
EF Core against Postgres (`ArticleNewsDbContext`), tables `article_url` and `article_ticker_sentiment` (snake_case columns, configured via `OnModelCreating`). `ArticleUrlRepository` uses `IDbContextFactory` to create a fresh, short-lived `DbContext` per operation rather than a scoped injected context.

Schema is owned by **FluentMigrator**, same as the other TTM services: migrations live in `article_news_raw/Migrations/` (inside the web project, not a separate project), the runner is registered in `DiContainer.AddCustomServices` alongside the `DbContextFactory`, and `Program.cs` calls `runner.MigrateUp()` at startup before the Hangfire jobs are set up. Migrations derive from `ForwardOnlyMigration` (no `Down()`) and are numbered `yyyyMMdd_HHmm`. There are no EF migrations — the `DbContext` must be kept in sync with the migrations by hand.

`article_url` / `article_ticker_sentiment` predate FluentMigrator and already exist in the live database, so the baseline migration (`20260816_1200_BaselineArticleTables`) guards each `Create.Table` with `Schema.Table(...).Exists()` — a no-op against the live DB, but it lets a fresh database be provisioned from code. Two gotchas when adding tables here: Npgsql maps `DateTime` to `timestamp with time zone`, which `.AsDateTime()` does *not* produce (use `.AsCustom("timestamp with time zone")`), and `.Identity()` emits `GENERATED ALWAYS AS IDENTITY` while the `DbContext` declares `UseIdentityByDefaultColumn()` (the baseline corrects this with an `ALTER ... SET GENERATED BY DEFAULT`).

`commodities` (`date`, `commodity_type`, `value`) is created by `20260816_1210_Commodities`, with a composite primary key on `(date, commodity_type)` so repeated monthly fetches are idempotent. `commodity_type` holds `GOLD` / `SILVER` / `BRENT`. Nothing writes to it yet — see below.

### Commodities (partially wired)
`AlphaVantageCommoditiesService` (DataAccess) fetches **monthly** commodity history from AlphaVantage: `GetGoldHistory()` and `GetSilverHistory()` hit `function=GOLD_SILVER_HISTORY` with `symbol=GOLD`/`SILVER`, `GetBrentCrudeOilHistory()` hits `function=BRENT`. It reuses `ALPHAVANTAGE_API_KEY` and the `HttpClientExtensions.GetJson` helper, and returns `AlphaVantageCommodity` (`{name, interval, unit, data:[{date, value}]}`).

`datatype=json` is passed explicitly because `GOLD_SILVER_HISTORY` defaults to **csv** — do not remove it. Values come back as quoted strings and must be parsed with `InvariantCulture`; AlphaVantage uses `"."` for a missing observation. As with the news service, a rate-limited call returns HTTP 200 with an `{"Information": ...}` body, which deserializes to an object with a null `Data`, so null-guard it.

The service is registered in DI but **nothing calls it yet** — there is no entity, repository, handler, Hangfire job, or controller endpoint for commodities. That persistence layer is the intended next step.

### Unused/in-progress code
`SitemapService` (fetches and recursively parses XML sitemaps, with gzip/deflate/brotli decompression handling) exists but is not registered in DI or referenced by any handler — treat it as scaffolding for a not-yet-wired sitemap-based ingestion path.
