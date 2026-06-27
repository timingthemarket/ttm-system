# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Build and Run
- **Build solution**: `dotnet build boersdata_raw.sln`
- **Run main application**: `dotnet run --project boersdata_raw/boersdata_raw.csproj`
- **Run tests**: `dotnet test boersdata_raw.Tests/boersdata_raw.Tests.csproj`
- **Run single test**: `dotnet test --filter "TestName"`

### Docker
- **Build Docker image**: `docker build --tag 'boersdata-raw' .`
- **Run Docker container**: `docker run --detach 'boersdata-raw'`
- **Docker Compose**: `docker-compose up -d`

### Test Framework
- Uses xUnit for testing with AutoBogus and Bogus for test data generation
- Test coverage with coverlet.collector
- Run tests with: `dotnet test` or `dotnet test --collect:"XPlat Code Coverage"`

## Architecture Overview

### Project Structure
The solution follows a clean architecture pattern with four main projects:

1. **boersdata_raw** (Main API) - ASP.NET Core Web API with gRPC support
2. **boersdata_raw.DataAccess** - Data layer with MongoDB repositories and external API integrations
3. **boersdata_raw.Domain** - Business logic handlers and domain models
4. **boersdata_raw.Tests** - Unit and integration tests

### Core Components

#### API Integration
- **BoersData API**: Primary data source for Nordic/Global financial instruments, stock prices, and reports
- **Yahoo Finance API**: Alternative/supplementary price data source
- All API calls use environment variables for configuration (BOERSDATA_API_KEY, BASE_URL_API)

#### Data Synchronization Flow
1. **Metadata Sync** (`/sync/metadata`) - Syncs countries, markets, sectors, industries
2. **Securities Sync** (`/sync/securities`) - Syncs financial instruments
3. **Historical Prices** (`/sync/historical-prices`) - Syncs 5 years of price history
4. **Reports Sync** (`/sync/reports-sync`) - Syncs financial reports (up to 10 years)

#### Background Services
- **DailyPricesService**: Processes daily price updates via queue
- **ReportsService**: Handles financial report synchronization
- **WeeklyRefreshPricesService**: Performs weekly price data refresh

#### Scheduled Jobs (Hangfire)
- Daily prices: Weekdays at 21:30 and 03:30 UTC
- Weekly reports: Sundays at 09:00 UTC  
- Weekly price refresh: Saturdays at 18:00 UTC

### Technology Stack
- **.NET 9.0** with nullable reference types enabled
- **MongoDB** for data persistence
- **Hangfire** for background job scheduling (in-memory storage)
- **gRPC** and **HTTP/1.1** endpoints (ports 5104/5004)
- **OpenTelemetry** for observability
- **MassTransit** for messaging/events

### Key Data Models
- **Security**: Financial instruments with metadata
- **StockPrice**: Historical and current pricing data
- **Report**: Financial statements (Income, Balance Sheet, Cash Flow, KPIs)
- **Market/Country/Sector**: Reference data for securities classification

### Environment Configuration
Required environment variables:
- `MONGODB_CONN_STRING`: MongoDB connection string
- `BOERSDATA_API_KEY`: API key for BoersData service
- `BASE_URL_API`: BoersData API base URL (defaults to https://apiservice.borsdata.se/)
- `OLT_ENDPOINT`: OpenTelemetry endpoint (defaults to http://localhost:4317)

### Development Notes
- Uses source generators for JSON serialization (BoersDataJsonSerializerGenerator)
- MongoDB repositories follow repository pattern with interfaces
- Exception handling via custom middleware (ExceptionLoggerMiddleware)
- gRPC reflection enabled for development
- Swagger/OpenAPI documentation available at runtime