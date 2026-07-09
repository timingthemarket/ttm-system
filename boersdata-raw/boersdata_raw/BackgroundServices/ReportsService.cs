using System.Diagnostics;
using boersdata_raw.Domain.Interfaces;
using boersdata_raw.Domain.Models;
using boersdata_raw.Domain.Queue;
using MassTransit;
using TTM.Shared.Constants;
using TTM.Shared.Events.BoersDataRaw;
using TTM.Shared.Extensions;

namespace boersdata_raw.BackgroundServices;

public class ReportsService(
    ILogger<ReportsService> logger,
    IServiceProvider serviceProvider,
    IQueueCache<ReportsQueue> queue)
    : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("boersdata_raw.BackgroundServices.ReportsService");
    
    private bool _hasRunFirstTime;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = ActivitySource.StartActivity("ReportsService.ExecutionLoop");
            
            if (!_hasRunFirstTime)
            {
                logger.LogInformation($"Starting up {nameof(ReportsService)}...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                _hasRunFirstTime = true;
                activity?.SetTag("service.startup", "true");
            }

            using var scope = serviceProvider.CreateScope();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            try
            {
                var queuedItem = queue.DequeueAndGetItem();
                activity?.SetTag("queue.has_item", queuedItem != null);
                
                if (queuedItem != null)
                {
                    using var processActivity = ActivitySource.StartActivity("ReportsService.HandleSyncReports");
                    
                    logger.LogInformation("Dequeued item from 'ReportsQueue'");
                    var securitiesReportsHandler =
                        scope.ServiceProvider.GetRequiredService<ISyncSecuritiesReportsHandler>();

                    var reports = await securitiesReportsHandler.HandleSyncReports();
                    processActivity?.SetTag("reports.count", reports?.Count ?? 0);
                    
                    await publishEndpoint.Publish(new RawReportsSyncCompleteEvent
                    {
                        Reports = reports
                    }, stoppingToken);
                    processActivity?.SetTag("event.type", "RawReportsSyncCompleteEvent");
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
                logger.LogError(e, "Task cancelled");
            }
            catch (Exception e)
            {
                activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                await publishEndpoint.SendSystemError(e, SharedSettings.AppName);
                logger.LogError(e, "Unhandled exception occured");
            }
        }
    }
}