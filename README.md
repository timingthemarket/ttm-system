# TTM — Timing The Market

A financial data and portfolio simulation platform, built as a set of independent .NET microservices
that ingest market data from external providers, refine it into master data, and use it to run
portfolio simulations and strategies.

This repository is a **polyrepo-style monorepo**: each service has its own `.sln` and `Dockerfile`
and is built, tested and deployed on its own. There is no root solution file and no root
`docker-compose.yml` tying everything together.

## Services

| Directory | What it does |
|---|---|
| `TTM.Shared` | Shared library (event contracts, gRPC interfaces, models, logging/telemetry extensions) referenced by every service |
| `boersdata-raw` | Ingests securities, prices and financial reports from the BoersData and Yahoo Finance APIs (MongoDB) |
| `riksbanken-raw` | Ingests currency exchange rates from Riksbanken, Sweden's central bank (Marten) |
| `article-news-raw` | Ingests financial news articles and sentiment (Finnhub, AlphaVantage) |
| `securities-masterdata` | Master data: securities, prices, technical indicators, currency rates. Consumes events from the raw services, serves REST + gRPC |
| `portfolio` | Portfolio simulation and optimization engine with pluggable strategies. `portfolio.Explorer` is a separate worker for exploration runs |
| `infra-observability` | Central log/metric/error sink for the other services, plus the local infrastructure `docker-compose.yml` (Postgres, MongoDB, RabbitMQ) |

Each service directory has its own `CLAUDE.md` with service-specific detail — architecture,
environment variables, scheduled jobs. Start there when working inside a service.

## How it fits together

```
 BoersData / Yahoo ──▶ boersdata-raw ──┐
 Riksbanken ─────────▶ riksbanken-raw ─┼─ events (MassTransit/RabbitMQ) ─▶ securities-masterdata
 Finnhub / AlphaVantage ▶ article-news-raw                                        │
                                                                          gRPC   │
                                                                                 ▼
                                                                            portfolio
                                                                     (simulations, strategies)

 all services ──▶ infra-observability (logs, metrics, errors)  +  OTLP collector (traces)
```

1. The **raw ingestion services** pull from an external API on a Hangfire schedule and persist to
   their own database.
2. They publish domain events over **MassTransit + RabbitMQ**.
3. **securities-masterdata** consumes those events, builds master data and calculates technical
   indicators, and exposes it over REST and gRPC.
4. **portfolio** reads master data over gRPC (`MASTERDATA_URL`) and runs simulations.
5. Everything ships logs/metrics to **infra-observability** and traces to an OTLP collector.

## Tech stack

- **.NET 10** / ASP.NET Core (`TTM.Shared` multi-targets `net8.0;net10.0`)
- **MassTransit + RabbitMQ** — event bus between services
- **protobuf-net.Grpc** — request/response calls between services
- **PostgreSQL** (EF Core + FluentMigrator) for most services; **MongoDB** for `boersdata-raw`;
  **Marten** for `riksbanken-raw`
- **Hangfire** (in-memory storage) — recurring jobs: daily price syncs, weekly refreshes, report syncs
- **Serilog + OpenTelemetry** — wired up through `TTM.Shared` extensions (`AddTtmTracing`, `AddTtmOtelLogger`)
- **xUnit** with AutoBogus/Bogus for tests

## Conventions

- **Clean-Architecture shape.** Each service is split into an API host project, `.Domain`
  (handlers/business logic), `.DataAccess` (repositories, EF/Marten/Mongo), and usually `.Tests`.
- **Event contracts live in `TTM.Shared/TTM.Shared/Events/`**, grouped by originating domain
  (`BoersDataRaw`, `RiksbankenRaw`, `PortfolioSimulation`, `SecuritiesMasterdata`, `Infra`).
  MassTransit maps events by **full namespace + class name**, so both must match exactly on the
  publishing and consuming side — services reference the same `TTM.Shared` project rather than
  sharing an assembly at runtime.
- **Databases are per-service.** No service reads another service's database; cross-service data
  moves over events or gRPC.
- **Migrations** run automatically on startup (FluentMigrator), named `YYYYMMDD_HHMM_Description.cs`.

### Making a cross-service change

A new event type or gRPC method usually touches three places, in this order:

1. `TTM.Shared` — add/change the contract
2. the publishing service
3. the consuming service(s)

Read `TTM.Shared/CLAUDE.md` and the relevant service `CLAUDE.md` before starting.
