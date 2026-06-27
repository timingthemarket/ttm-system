using System.Diagnostics;
using boersdata_raw.Domain.Interfaces;
using boersdata_raw.Domain.Models;
using boersdata_raw.Domain.Queue;
using MassTransit;
using TTM.Shared.Constants;
using TTM.Shared.Events.BoersDataRaw;
using TTM.Shared.Extensions;

namespace boersdata_raw.BackgroundServices;

public class DailyPricesService : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("boersdata_raw.BackgroundServices.DailyPricesService");
    
    private readonly ILogger<DailyPricesService> _logger;
    private readonly IQueueCache<DailyPricesQueue> _queue;
    private readonly IServiceProvider _serviceProvider;

    private bool _hasRunFirstTime;

    public DailyPricesService(ILogger<DailyPricesService> logger, IServiceProvider serviceProvider,
        IQueueCache<DailyPricesQueue> queue)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = ActivitySource.StartActivity("DailyPricesService.ExecutionLoop");
            
            if (!_hasRunFirstTime)
            {
                _logger.LogInformation($"Starting up {nameof(DailyPricesService)}...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                _hasRunFirstTime = true;
                activity?.SetTag("service.startup", "true");
            }

            using var scope = _serviceProvider.CreateScope();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            try
            {
                var queuedItem = _queue.DequeueAndGetItem();
                activity?.SetTag("queue.has_item", queuedItem != null);
                
                if (queuedItem != null)
                {
                    using var processActivity = ActivitySource.StartActivity("DailyPricesService.HandleDailyPricesSync");
                    
                    _logger.LogInformation("Dequeued item from 'DailyPricesQueue'");
                    var securitiesDailyPricesHandler =
                        scope.ServiceProvider.GetRequiredService<ISyncSecuritiesDailyPricesHandler>();

                    var dailyPrices = await securitiesDailyPricesHandler.HandleDailyPricesSync();
                    processActivity?.SetTag("daily_prices.count", dailyPrices?.Count ?? 0);
                    
                    await publishEndpoint.Publish(new RawDailyPricesSyncCompleteEvent
                    {
                        DailyPrices = dailyPrices
                    });
                    processActivity?.SetTag("event.type", "RawDailyPricesSyncCompleteEvent");
                }
                else
                {
                    var waitTs = TimeSpan.FromSeconds(5);
                    await Task.Delay(waitTs, stoppingToken);
                    activity?.SetTag("queue.wait_seconds", 5);
                }
            }
            catch (TaskCanceledException e)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Task cancelled");
                _logger.LogError(e, "Task cancelled");
            }
            catch (Exception e)
            {
                activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                await publishEndpoint.SendSystemError(e, SharedSettings.AppName);
                _logger.LogError(e, "Unhandled exception occured");
            }
        }
    }
}