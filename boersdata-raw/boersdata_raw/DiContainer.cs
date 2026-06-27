using System.Diagnostics.CodeAnalysis;
using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Repositories;
using boersdata_raw.DataAccess.Services;
using boersdata_raw.Domain.Handlers.Query;
using boersdata_raw.Domain.Handlers.Sync;
using boersdata_raw.Domain.Interfaces;
using boersdata_raw.Domain.Models;
using boersdata_raw.Domain.Queue;
using boersdata_raw.Domain.Services;
using boersdata_raw.Filters;
using MassTransit;
using TTM.Shared.Constants;

namespace boersdata_raw;

[ExcludeFromCodeCoverage]
public static class DiContainer
{
    public static void ConfigureMasstransit(this IServiceCollection service)
    {
        var localhostAddress = Environment.GetEnvironmentVariable("DOCKER_RABBITMQ_ACCESS") ?? "localhost";
        service.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.UseConsumeFilter(typeof(ExceptionFilter<>), context);
                
                cfg.Host(localhostAddress, "/", h =>
                {
                    h.Username("user");
                    h.Password("password");
                });
                cfg.ConfigureEndpoints(context);
            });
        });
    }

    public static void AddCustomServices(this IServiceCollection service)
    {
        service.ConfigureMasstransit();
        service.AddMemoryCache();

        // Database
        service.AddSingleton<ICountryRepository, CountryRepository>();
        service.AddSingleton<IMarketRepository, MarketRepository>();
        service.AddSingleton<ISectorRepository, SectorRepository>();
        service.AddSingleton<IStockPricesRepository, StockPricesRepository>();
        service.AddSingleton<IReportRepository, ReportRepository>();
        service.AddSingleton<ISecuritiesRepository, SecuritiesRepository>();

        // Services
        service.AddTransient<IBoersDataService, BoersDataService>();

        // handlers
        service.AddScoped<ISyncSecurityMetadataHandler, SyncSecuritiesMetadataHandler>();
        service.AddScoped<ISyncSecuritiesHistoricalPricesHandler, SyncHistoricalPricesHandler>();
        service.AddScoped<ISyncSecuritiesHandler, SyncSecuritiesHandler>();
        service.AddScoped<ISyncSecuritiesReportsHandler, SyncReportsHandler>();
        service.AddScoped<ISyncSecuritiesDailyPricesHandler, SyncDailyPricesHandler>();

        service.AddScoped<IQryHistoricalReportsHandler, QryHistoricalReportsHandler>();
        service.AddScoped<IQryHistoricalSecuritiesPricesHandler, QryHistoricalSecuritiesPricesHandler>();
        service.AddScoped<IQrySecuritiesHandler, QrySecuritiesHandler>();
        
        //Queues
        service.AddSingleton<IQueueCache<DailyPricesQueue>, QueueCache<DailyPricesQueue>>();
        service.AddSingleton<IQueueCache<ReportsQueue>, QueueCache<ReportsQueue>>();
        service.AddSingleton<IQueueCache<WeeklyRefreshPricesQueue>, QueueCache<WeeklyRefreshPricesQueue>>();
    }
}