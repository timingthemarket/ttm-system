# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

This is a .NET shared library project that targets both .NET 8.0 and .NET 9.0.

### Building
```bash
dotnet build TTM.Shared.sln
dotnet build TTM.Shared/TTM.Shared.csproj
```

### Package Generation
The project is configured to generate NuGet packages automatically on build (`GeneratePackageOnBuild=true`).

### Testing
Since this is a shared library, tests would typically be in a separate test project. Look for test projects in the solution or use:
```bash
dotnet test
```

## Architecture Overview

TTM.Shared is a .NET shared library that provides common contracts, extensions, and utilities for the TTM (Timing The Market) system. The library is organized into several key areas:

### Core Structure
- **Constants/**: Enums and constants for system-wide values (Direction, EventOrigin, Indicators, etc.)
- **Events/**: Event contracts for different domains (BoersDataRaw, Infra, Portfolio, etc.)
- **Models/**: Data transfer objects and query models organized by domain
- **Extensions/**: Utility extensions for logging, OpenTelemetry, and gRPC
- **gRPC/**: Service interfaces for gRPC communication

### Key Architectural Patterns
- **Domain-Driven Organization**: Code is organized by business domains (BoersDataRaw, SecuritiesMasterdata, PortfolioSimulation)
- **Event-Driven Architecture**: Extensive use of events for system communication
- **gRPC Communication**: Service interfaces defined using protobuf-net.Grpc
- **OpenTelemetry Integration**: Built-in observability through OtelExtension
- **MassTransit Integration**: Message bus integration for event handling

### Technology Stack
- Multi-target framework: .NET 8.0 and .NET 9.0
- gRPC with protobuf-net
- MassTransit with RabbitMQ
- OpenTelemetry for observability
- Serilog for logging
- FluentAssertions and NSubstitute for testing

### Key Dependencies
- **Grpc.Net.Client**: gRPC client functionality
- **MassTransit.RabbitMQ**: Message bus integration
- **OpenTelemetry**: Distributed tracing and metrics
- **Serilog**: Structured logging with various sinks
- **protobuf-net.Grpc**: Protocol buffer support for gRPC

### Domain Models
- **BoersDataRaw**: Historical price and report data queries
- **SecuritiesMasterdata**: Security information and indicators
- **PortfolioSimulation**: Portfolio simulation commands and DTOs
- **Infra**: Infrastructure logging and metrics models

The library serves as the foundation for microservices in the TTM system, providing shared contracts and cross-cutting concerns.