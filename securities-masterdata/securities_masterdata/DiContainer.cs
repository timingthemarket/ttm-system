using System.Diagnostics.CodeAnalysis;
using MassTransit;
using securities_masterdata.Consumers.BoersDataRaw;
using securities_masterdata.Consumers.Internal;
using securities_masterdata.Consumers.RiksbankenRaw;
using securities_masterdata.DataAccess.Cache;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.DataAccess.Repositories;
using securities_masterdata.DataAccess.Services;
using securities_masterdata.Domain.Factory;
using securities_masterdata.Domain.Factory.Functions;
using securities_masterdata.Domain.Handlers.Commands;
using securities_masterdata.Domain.Handlers.Query;
using securities_masterdata.Domain.Handlers.Sync;
using securities_masterdata.Domain.Handlers.Sync.Backfill;
using securities_masterdata.Domain.Interfaces;
using securities_masterdata.Filters;
using securities_masterdata.Services;
using TTM.Shared.Events.BoersDataRaw;

namespace securities_masterdata;

[ExcludeFromCodeCoverage]
public static class DiContainer
{
    public static void ConfigureMasstransit(this IServiceCollection service)
    {
        var localhostAddress = Environment.GetEnvironmentVariable("DOCKER_RABBITMQ_ACCESS") ?? "localhost";
        Console.WriteLine($"Listening on: {localhostAddress}");
        service.AddMassTransit(x =>
        {
            x.AddConsumer<RawReportsSyncCompleteEventConsumer>();
            x.AddConsumer<RawDailyPricesSyncCompleteEventConsumer>();
            x.AddConsumer<SyncDailyCurrencyRatesConsumer>();
            
            x.AddConsumer<SyncDailyPricesCompleteInternalConsumer>();
            
            x.AddConsumer<HistoricalPricesSyncCompleteEventConsumer>();
            
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingInMemory();
            
            /*x.UsingRabbitMq((context, cfg) =>
            {
                cfg.UseConsumeFilter(typeof(ExceptionFilter<>), context);

                cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
                
                cfg.Host(localhostAddress, "/", h =>
                {
                    h.Username("user");
                    h.Password("password");
                });
                
                cfg.ConfigureEndpoints(context);
            });*/
        });
    }

    public static void AddCustomServices(this IServiceCollection service)
    {
        service.ConfigureMasstransit();
        service.AddMemoryCache();

        // Database
        service.AddScoped<ICurrencyRepository, CurrencyRepository>();
        service.AddScoped<IMarketRepository, MarketRepository>();
        service.AddScoped<ISecurityRepository, SecurityRepository>();
        service.AddScoped<IIndicatorsRepository, IndicatorsRepository>();
        service.AddScoped<IIndexRepository, IndexRepository>();
        
        // External Services
        service.AddHttpClient<IAvanzaService, AvanzaService>();
        service.AddHttpClient<INordnetService, NordnetService>();
        
        // Factories
        service.AddScoped<IIndicatorsCalculationFactory, IndicatorsCalculationFactory>();

        service.AddScoped<IFactoryFunction, BetaOmx30Function>();
        service.AddScoped<IFactoryFunction, ReturnFunction>();
        service.AddScoped<IFactoryFunction, VolatilityFunction>();
        service.AddScoped<IFactoryFunction, RsiMomentumFunction>();
        service.AddScoped<IFactoryFunction, PeFunction>();
        
        // Cache
        service.AddSingleton<SecuritiesPricesCache>();
        service.AddSingleton<IndicatorsCache>();
        
        // Queue Services
        service.AddSingleton<IBackfillQueueService, BackfillQueueService>();

        // handlers
        service.AddScoped<ICmdAddIndexSecurityHandler, CmdAddIndexSecurityHandler>();
        
        service.AddScoped<IQrySecuritiesPricesHandler, QrySecuritiesPricesHandler>();
        service.AddScoped<IQrySecuritiesIndicatorsHandler, QrySecuritiesIndicatorsForPortfolioHandler>();
        service.AddScoped<IQrySecuritiesHandler, QrySecuritiesHandler>();
        
        service.AddScoped<IDailyPricesHandler, DailyPricesHandler>();
        service.AddScoped<IDailyCurrencyRatesHandler, DailyCurrencyRatesHandler>();
        service.AddScoped<IBackfillCurrencyRatesHandler, BackfillCurrencyRatesHandler>();
        service.AddScoped<IBackfillSecuritiesHandler, BackfillSecuritiesHandler>();
        service.AddScoped<IBackfillSecuritiesPricesHandler, BackfillSecuritiesPricesHandler>();
        service.AddScoped<IBackfillReportsHandler, BackfillReportsHandler>();
        service.AddScoped<IPricesIndexHandler, PricesIndexHandler>();
        service.AddScoped<IAvanzaSyncHandler, AvanzaSyncHandler>();
    }
}