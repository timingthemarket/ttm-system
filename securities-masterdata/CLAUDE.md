# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

### Building the Application
```bash
# Build the entire solution
dotnet build securities_masterdata.sln

# Build specific project
dotnet build securities_masterdata/securities_masterdata.csproj
```

### Running Tests
```bash
# Run all tests
dotnet test securities_masterdata.Tests/securities_masterdata.Tests.csproj

# Run tests with coverage
dotnet test securities_masterdata.Tests/securities_masterdata.Tests.csproj --settings coverlet.runsettings
```

### Running the Application
```bash
# Run locally
dotnet run --project securities_masterdata/securities_masterdata.csproj

# Docker build
docker build --tag 'securities-masterdata' .

# Docker compose
sudo docker-compose --compatibility up -d --build
```

## Project Architecture

This is a **Securities Master Data Service** built with .NET 9 that provides financial data management capabilities including securities prices, indicators, and currency rates.

### Project Structure

The solution follows a **Clean Architecture** pattern with these main projects:

- **securities_masterdata** - Main API service (Web API + gRPC)
- **securities_masterdata.Domain** - Business logic and handlers
- **securities_masterdata.DataAccess** - Data layer with Entity Framework
- **securities_masterdata.Shared** - Shared models and events
- **securities_masterdata.Tests** - Unit tests using xUnit

### Key Technologies and Patterns

**Data Access:**
- Entity Framework Core with PostgreSQL
- FluentMigrator for database migrations
- Repository pattern for data access

**Messaging:**
- MassTransit with RabbitMQ for event-driven architecture
- Event consumers for handling external system events
- Internal events for cross-service communication

**API Design:**
- REST API controllers for HTTP endpoints
- gRPC services for high-performance inter-service communication
- Swagger/OpenAPI documentation

**Financial Calculations:**
- Skender.Stock.Indicators library for technical analysis
- Factory pattern for indicator calculations
- Support for various financial indicators (RSI, Volatility, Beta, etc.)

### Core Domain Entities

- **Security** - Financial instruments (stocks, bonds, etc.)
- **SecurityPrice** - Historical and current prices
- **Indicator** - Calculated financial indicators
- **Index** - Market indexes and their constituents
- **Currency/CurrencyRate** - Currency exchange rates

### Message Consumers

The service listens for events from external systems:
- **BoersDataRaw** events for price and report synchronization
- **RiksbankenRaw** events for currency rate updates
- Internal events for coordinating data processing

### Environment Variables

Key environment variables required:
- `POSTGRESSQL_CONN` - Database connection string
- `DOCKER_RABBITMQ_ACCESS` - RabbitMQ host
- `BOERSDATA_URL` - External service URL
- `INFRA_SERVICE_URL` - Infrastructure service endpoint
- `OLT_ENDPOINT` - OpenTelemetry endpoint

### Event Contracts

When adding new event contracts, place them in `/Events` folder in the `.Shared` project. MassTransit maps events by namespace and classname, so ensure consistency across services.

### Cache Implementation

The service uses an in-memory cache (`SecuritiesPricesCache`) managed by a background worker (`PriceCacheWorker`) for performance optimization of frequently accessed price data.