# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Building and Running
- **Build solution**: `dotnet build`
- **Run main application**: `dotnet run --project portfolio` (runs on port 5500 in development, 5006 in production)
- **Run explorer worker**: `dotnet run --project portfolio.Explorer`
- **Run tests**: `dotnet test`
- **Run specific test**: `dotnet test --filter "TestMethodName"`

## Architecture Overview

This is a .NET 9 portfolio management system with the following structure:

### Core Projects
- **portfolio**: Main web API application with controllers, background services, and Hangfire scheduler
- **portfolio.Domain**: Business logic, handlers, and domain models
- **portfolio.DataAccess**: Entity Framework database context, repositories, and data models
- **portfolio.Explorer**: Background worker service for portfolio exploration calculations
- **portfolio.Tests**: Unit tests using xUnit

### Key Components
- **Portfolio Calculation Engine**: Core portfolio optimization and calculation logic in `portfolio.Domain/Portfolio/`
- **Strategy System**: Pluggable strategy implementations in `portfolio.Domain/Portfolio/Factory/StrategyModules/`
- **Message Bus**: Uses MassTransit with RabbitMQ for inter-service communication
- **Background Processing**: Hangfire for scheduled jobs, hosted services for continuous processing
- **Database**: PostgreSQL with FluentMigrator for schema management

### External Dependencies
- **Database**: Requires `POSTGRESSQL_CONN` environment variable
- **Message Queue**: RabbitMQ (configurable via `DOCKER_RABBITMQ_ACCESS`)
- **Telemetry**: OpenTelemetry endpoint via `OLT_ENDPOINT`
- **Masterdata Service**: gRPC service via `MASTERDATA_URL`

### Key Patterns
- **CQRS**: Separate command/query handlers in `portfolio.Domain/Handlers/`
- **Dependency Injection**: All services registered in `portfolio.Domain/DiContainer.cs`
- **Repository Pattern**: Data access abstracted through interfaces in `portfolio.DataAccess/Interfaces/`
- **Factory Pattern**: Strategy and allocator factories for pluggable algorithms

### Database Migrations
- Migrations are in `portfolio/Migrations/` using FluentMigrator
- Automatically run on application startup
- Follow naming convention: `YYYYMMDD_HHMM_Description.cs`

### Testing
- Unit tests use xUnit with AutoBogus for test data generation
- Integration tests use `Microsoft.AspNetCore.Mvc.Testing`
- Test coverage reports use coverlet