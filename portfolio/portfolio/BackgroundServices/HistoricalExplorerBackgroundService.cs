using MassTransit;
using portfolio.Domain.Interfaces;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;

namespace portfolio.BackgroundServices;

public class HistoricalExplorerBackgroundService(ILogger<HistoricalExplorerBackgroundService> logger, IServiceProvider serviceProvider)
    : BackgroundService
{
    private bool _hasRunFirstTime = false;
    private const int RestartDelaySeconds = 30;
    private const int MaxConcurrentTasks = 2;
    private readonly SemaphoreSlim _semaphore = new(MaxConcurrentTasks, MaxConcurrentTasks);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        
        // Outer loop for restart functionality
        while (true)
        {
            try
            {
                if (!_hasRunFirstTime)
                {
                    logger.LogInformation("Starting up historical explorer service...");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    _hasRunFirstTime = true;
                }
                
                var tasks = new List<Task>();
                
                while (!stoppingToken.IsCancellationRequested)
                {
                    await _semaphore.WaitAsync(stoppingToken);
                    
                    var workTask = Task.Run(async () =>
                    {
                        try
                        {
                            await ExecuteWorkCycle(stoppingToken);
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }, stoppingToken);
                    
                    tasks.Add(workTask);
                    
                    tasks.RemoveAll(t => t.IsCompleted);
                    
                    if (tasks.Count >= MaxConcurrentTasks)
                    {
                        await Task.WhenAny(tasks);
                        tasks.RemoveAll(t => t.IsCompleted);
                    }
                }
                
                if (tasks.Any())
                {
                    await Task.WhenAll(tasks);
                }
                
                // If we reach here, cancellation was requested
                if (stoppingToken.IsCancellationRequested)
                {
                    logger.LogWarning("HistoricalExplorerBackgroundService cancellation detected (stoppingToken.IsCancellationRequested = true). Service will restart in {RestartDelay} seconds.", RestartDelaySeconds);
                    
                    // Wait before restarting
                    await Task.Delay(TimeSpan.FromSeconds(RestartDelaySeconds));
                    
                    logger.LogInformation("Restarting HistoricalExplorerBackgroundService background service...");
                    continue; // Restart the service
                }
                
                break; // Normal shutdown
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("HistoricalExplorerBackgroundService received OperationCanceledException. Service will restart in {RestartDelay} seconds.", RestartDelaySeconds);
                
                // Wait before restarting
                await Task.Delay(TimeSpan.FromSeconds(RestartDelaySeconds));
                
                logger.LogInformation("Restarting HistoricalExplorerBackgroundService background service after OperationCanceledException...");
                continue; // Restart the service
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in HistoricalExplorerBackgroundService. Service will restart in {RestartDelay} seconds.", RestartDelaySeconds);
                
                using (IServiceScope scope = serviceProvider.CreateScope())
                {
                    var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                    await publishEndpoint.SendSystemError(ex, SharedSettings.AppName);
                }
                
                await Task.Delay(TimeSpan.FromSeconds(RestartDelaySeconds));
                
                logger.LogInformation("Restarting HistoricalExplorerBackgroundService background service after unexpected error...");
                continue; // Restart the service
            }
        }
    }

    private async Task ExecuteWorkCycle(CancellationToken stoppingToken)
    {
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            var historicalExplorerHandler = scope.ServiceProvider.GetRequiredService<IHistoricalExplorerHandler>();

            try
            {
                var result = await historicalExplorerHandler.ProcessHistoricalExplorerFromQueue(stoppingToken);

                if (result)
                {
                    logger.LogInformation("Historical exploration request completed!");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unhandled exception occurred in historical exploration processing");
            }
        }

        TimeSpan waitTs = TimeSpan.FromSeconds(5);
        await Task.Delay(waitTs, stoppingToken);
    }

    public override void Dispose()
    {
        _semaphore?.Dispose();
        base.Dispose();
    }
}