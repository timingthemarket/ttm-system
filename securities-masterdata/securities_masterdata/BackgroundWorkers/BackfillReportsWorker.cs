using securities_masterdata.Domain.Interfaces;
using securities_masterdata.Services;

namespace securities_masterdata.BackgroundWorkers;

public class BackfillReportsWorker(
    ILogger<BackfillReportsWorker> logger,
    IServiceProvider serviceProvider,
    IBackfillQueueService queueService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        logger.LogInformation("BackfillReportsWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (queueService.TryDequeueBackfillRequest())
                {
                    await ProcessBackfillRequest(stoppingToken);
                }
                
                // Check for new requests every 5 seconds
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing backfill queue");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Wait longer on error
            }
        }

        logger.LogInformation("BackfillReportsWorker stopped");
    }

    private async Task ProcessBackfillRequest(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var backfillHandler = scope.ServiceProvider.GetRequiredService<IBackfillReportsHandler>();

        logger.LogInformation("Processing backfill reports request");
        
        try
        {
            await backfillHandler.HandleBackfillReports();
            logger.LogInformation("Backfill reports request processed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing backfill reports");
        }
    }
}