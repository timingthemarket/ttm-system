using System.Diagnostics.CodeAnalysis;
using MassTransit;
using riksbanken_raw.Consumers;
using riksbanken_raw.DataAccess.Interfaces;
using riksbanken_raw.DataAccess.Repositories;
using riksbanken_raw.DataAccess.Services;
using riksbanken_raw.Domain.Handlers.Query;
using riksbanken_raw.Domain.Handlers.Sync;
using riksbanken_raw.Domain.Interfaces;
using riksbanken_raw.Filters;
using riksbanken_raw.Triggers;
using TTM.Shared.Constants;

namespace riksbanken_raw;

[ExcludeFromCodeCoverage]
public static class DiContainer
{
    public static IServiceCollection Create()
    {
        var services = new ServiceCollection();
        services.AddCustomServices();

        return services;
    }

    public static void ConfigureMasstransit(this IServiceCollection service)
    {
        var localhostAddress = Environment.GetEnvironmentVariable("DOCKER_RABBITMQ_ACCESS") ?? "localhost";
        service.AddMassTransit(x =>
        {
            x.AddConsumer<HistoricalCurrenciesQryConsumer>();

            x.AddConsumer<SyncDailyCurrencyRatesTrigger>();

            x.SetKebabCaseEndpointNameFormatter();
            x.UsingInMemory();
            
            /*x.UsingRabbitMq((context, cfg) =>
            {
                cfg.UseConsumeFilter(typeof(ExceptionFilter<>), context);

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
        service.AddHttpClient();

        // Database
        service.AddSingleton<IRiksbankenRepository, RiksbankenRepository>();

        // Services
        service.AddSingleton<IRiksbankenService, RiksbankenService>();

        // handlers
        service.AddScoped<IHistoricalCurrencySyncHandler, SyncHistoricalCurrencyHandler>();
        service.AddScoped<ICurrencySyncHandler, SyncCurrencyHandler>();
        service.AddScoped<ICurrencyQryHandler, QryCurrencyHandler>();
    }
}