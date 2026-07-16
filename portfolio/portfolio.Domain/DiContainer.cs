using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using portfolio.DataAccess.Interfaces;
using portfolio.DataAccess.Repositories;
using portfolio.DataAccess.Services;
using portfolio.Domain.Handlers;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Portfolio.Factory;
using portfolio.Domain.Portfolio.Factory.StrategyModules;
using portfolio.Domain.Queue;
using portfolio.Domain.Services;
using ProtoBuf.Grpc.Client;
using ProtoBuf.Grpc.ClientFactory;

namespace portfolio.Domain;

[ExcludeFromCodeCoverage]
public static class DiContainer
{
    public static void AddCustomServices(this IServiceCollection service)
    {
        /*
        var redisHost = Environment.GetEnvironmentVariable("REDIS_CONN") ?? throw new Exception("No REDIS_CONN");
        service.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = new ConfigurationOptions
            {
                ReconnectRetryPolicy = new LinearRetry(5000),
                AbortOnConnectFail = false,
                EndPoints = { { redisHost, 6379 } }
            };
            options.InstanceName = "portfolio-";
        });
        */

        // GRPC Clients
        var masterdataUrl = Environment.GetEnvironmentVariable("MASTERDATA_URL") ?? "http://localhost:5101";

        GrpcClientFactory.AllowUnencryptedHttp2 = true;
        service.AddCodeFirstGrpcClient<TTM.Shared.gRPC.Services.IMasterdataService>(o => 
        { 
            o.Address = new Uri(masterdataUrl);
            o.CallOptionsActions.Add(op =>
            {
                op.CallOptions = new CallOptions(deadline: DateTime.UtcNow.AddSeconds(60));
            });
            o.ChannelOptionsActions.Add(options =>
            {
                options.MaxRetryAttempts = 3;
                options.DisposeHttpClient = true;
                options.HttpHandler = new SocketsHttpHandler()
                {
                    // keeps connection alive
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    ConnectTimeout = TimeSpan.FromSeconds(30),
                    // allows channel to add additional HTTP/2 connections
                    EnableMultipleHttp2Connections = true
                };
            });
        });
        
        
        
        service.AddMemoryCache();
        
        // Configure default HTTP client with timeout
        service.ConfigureHttpClientDefaults(builder =>
        {
            builder.ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        });

        //Database 
        service.AddScoped<IPortfolioRepository, PortfolioRepository>();
        service.AddScoped<IPortfolioTrendsRepository, PortfolioTrendsRepository>();
        service.AddScoped<ISimulationRepository, SimulationRepository>();

        // Factory
        service.AddScoped<IStrategy, DiLegacyStrategy>();
        service.AddScoped<IPortfolioStrategyFactory, PortfolioStrategyFactory>();
        
        // Queue
        service.AddSingleton(new SimulationQueueCache());
        service.AddSingleton(new HistoricalExplorerQueueCache());
        
        //services
        service.AddTransient<IMasterdataService, MasterdataService>();
        service.AddScoped<IPortfolioExplorerHandler, PortfolioExplorerService>();
        service.AddScoped<IYahooExportService, YahooExportService>();

        service.AddScoped<IPortfolioExplorerNotificationService, PortfolioExplorerNotificationService>();

        //Handler
        service.AddScoped<IComputePortfolioHandler, ComputePortfolioHandler>();
        service.AddScoped<IRegisterSimulationHandler, RegisterSimulationHandler>();
        service.AddScoped<IProcessSimulationHandler, ProcessSimulationHandler>();
        service.AddScoped<IQuerySimulationsHandler, QuerySimulationsHandler>();
        service.AddScoped<IYahooCsvFileHandler, YahooCsvFileHandler>();
        service.AddScoped<SessionDateHandler>();
        service.AddScoped<IHistoricalExplorerHandler, HistoricalExplorerHandler>();
        service.AddScoped<IPortfolioTrendsHandler, PortfolioTrendsHandler>();
        service.AddScoped<IPortfolioPerformanceHandler, PortfolioPerformanceHandler>();

        service.AddScoped<IPortfolio, Portfolio.Portfolio>();
    }
}