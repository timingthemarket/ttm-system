# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

`ttm-system` is a polyrepo-style monorepo for **Timing The Market (TTM)**, a financial data and portfolio simulation platform. It contains five independent .NET microservices plus a shared library. Each service has its own `.sln`, `Dockerfile`, and is deployed independently — there is no root-level solution file or docker-compose that ties them together.

| Directory | Service | Purpose |
|---|---|---|
| `TTM.Shared` | Shared library (NuGet package) | Common contracts, events, gRPC interfaces, extensions used by every service |
| `boersdata-raw` | boersdata-raw | Ingests securities, prices, and financial reports from the BoersData and Yahoo Finance APIs |
| `riksbanken-raw` | riksbanken-raw | Ingests currency exchange rate data from Riksbanken (Sweden's central bank) |
| `article-news-raw` | article-news-raw | Ingests financial news articles (Finnhub, AlphaVantage) |
| `securities-masterdata` | securities-masterdata | Master data service: securities, prices, technical indicators, currency rates; consumes events from the raw ingestion services |
| `portfolio` | portfolio | Portfolio simulation/optimization engine with pluggable strategies |

Each service directory has its own `CLAUDE.md` with service-specific commands and architecture detail — read that file when working inside a given service. This root file covers what's common across all of them.

## Common Development Commands

Every service follows the same pattern (run from inside the service directory, e.g. `cd boersdata-raw`):

```bash
# Build
dotnet build <service_name>.sln

# Run the main API
dotnet run --project <service_name>/<service_name>.csproj

# Run tests (not all services have a .Tests project — see the service's own CLAUDE.md)
dotnet test <service_name>.Tests/<service_name>.Tests.csproj
dotnet test --filter "TestName"   # single test

# Docker
docker build --tag '<service-name>' .
docker run --detach '<service-name>'
```

`TTM.Shared` is a library, not a runnable service — see `TTM.Shared/CLAUDE.md`.

## Architecture: How the Services Fit Together

This is an **event-driven microservices system** built on ASP.NET Core, with **MassTransit + RabbitMQ** as the backbone connecting services, and **TTM.Shared** as the contract layer everyone depends on.

### Data flow
1. **Raw ingestion services** (`boersdata-raw`, `riksbanken-raw`, `article-news-raw`) each pull data from an external API on a Hangfire schedule and persist it to their own database.
2. They publish domain events (e.g. price/report/currency sync events) via MassTransit.
3. **securities-masterdata** consumes those events to build master data (securities, prices, indicators, currency rates) and exposes it over REST + gRPC.
4. **portfolio** consumes master data (via gRPC to securities-masterdata, configured with `MASTERDATA_URL`) to run portfolio simulations and strategies; `portfolio.Explorer` is a separate background worker for exploration calculations.

### Shared conventions across services
- **Each service is Clean-Architecture-shaped**: a main API project, a `.Domain` project (handlers/business logic), a `.DataAccess` project (repositories/EF/Marten), and usually a `.Tests` project.
- **Event contracts live in `TTM.Shared/TTM.Shared/Events/`**, organized by originating domain (`BoersDataRaw`, `RiksbankenRaw`, `PortfolioSimulation`, `SecuritiesMasterdata`, `Infra`). **MassTransit maps events by full namespace + class name** — when adding or changing an event contract, the namespace and class name must match exactly across the publishing and consuming services, since they each reference the same `TTM.Shared` package rather than a shared assembly at runtime.
- **gRPC service interfaces** (protobuf-net.Grpc) also live in `TTM.Shared`, for direct request/response calls between services (e.g. portfolio → securities-masterdata) as opposed to fire-and-forget events.
- **Observability**: every service wires up Serilog + OpenTelemetry via `TTM.Shared` extensions (`AddTtmTracing`, `AddTtmOtelLogger`, etc.), shipping logs to a central infra service (`INFRA_SERVICE_URL`) and traces/metrics to an OTLP collector (`OLT_ENDPOINT`).
- **Scheduling**: background/recurring work (daily price syncs, weekly refreshes, report syncs) uses Hangfire with in-memory storage, configured per-service in a `Scheduler/` folder and wired up at startup in `Program.cs`.
- **Databases are per-service, not shared**: most services use PostgreSQL via Entity Framework + FluentMigrator (`POSTGRESSQL_CONN`); `boersdata-raw` uses MongoDB instead; `riksbanken-raw` uses Marten (document DB on top of Postgres) rather than EF.

### Ports (local dev, HTTP/gRPC combined via Kestrel)
- `riksbanken-raw`: 5005
- `portfolio`: 5006 (5500 in some dev configs — check the service's own CLAUDE.md)
- `article-news-raw`: 5007
- `boersdata-raw`: 5104/5004
- `securities-masterdata`: separate HTTP1/HTTP2 listeners — see its `Program.cs`

### When making cross-service changes
A change that spans services (e.g. a new event type, a new gRPC method) typically touches: `TTM.Shared` (contract) → the publishing service → the consuming service(s). Check `TTM.Shared/CLAUDE.md` and the relevant service `CLAUDE.md` files before starting.
