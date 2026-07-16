using MassTransit;
using portfolio.Domain.Interfaces;
using TTM.Shared.Constants;
using TTM.Shared.Events.PortfolioSimulation;
using TTM.Shared.Extensions;

namespace portfolio.BackgroundServices;

public class SimulationService(ILogger<SimulationService> logger, IServiceProvider serviceProvider)
    : BackgroundService
{
    private bool _hasRunFirstTime;
    private const int RestartDelaySeconds = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        
        // Outer loop for restart functionality
        while (true)
        {
            try
            {
                _hasRunFirstTime = false; // Reset for each restart
                
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ExecuteWorkCycle(stoppingToken);
                }
                
                // If we reach here, cancellation was requested
                if (stoppingToken.IsCancellationRequested)
                {
                    logger.LogWarning("SimulationService cancellation requested (stoppingToken.IsCancellationRequested = true). Service will restart in {RestartDelay} seconds.", RestartDelaySeconds);
                    
                    // Wait before restarting
                    await Task.Delay(TimeSpan.FromSeconds(RestartDelaySeconds));
                    
                    logger.LogInformation("Restarting SimulationService background service...");
                    continue; // Restart the service
                }
                
                break; // Normal shutdown
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("SimulationService received OperationCanceledException. Service will restart in {RestartDelay} seconds.", RestartDelaySeconds);
                
                // Wait before restarting
                await Task.Delay(TimeSpan.FromSeconds(RestartDelaySeconds));
                
                logger.LogInformation("Restarting SimulationService background service after OperationCanceledException...");
                continue; // Restart the service
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in SimulationService. Service will restart in {RestartDelay} seconds.", RestartDelaySeconds);
                
                using (IServiceScope scope = serviceProvider.CreateScope())
                {
                    var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                    await publishEndpoint.SendSystemError(ex, SharedSettings.AppName);
                }
                
                // Wait before restarting
                await Task.Delay(TimeSpan.FromSeconds(RestartDelaySeconds));
                
                logger.LogInformation("Restarting SimulationService background service after unexpected error...");
                continue; // Restart the service
            }
        }
    }

    private async Task ExecuteWorkCycle(CancellationToken stoppingToken)
    {
        if (!_hasRunFirstTime)
        {
            logger.LogInformation("Starting up service...");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            _hasRunFirstTime = true;
        }

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var processSimulationHandler = scope.ServiceProvider.GetRequiredService<IProcessSimulationHandler>();

            try
            {
                var result = await processSimulationHandler.HandleProcessSimulationFromQueue();

                if (result != null)
                {
                    logger.LogInformation("Simulation {Id} complete!", result.Id);
                }
            }
            catch (Exception e)
            {
                await publishEndpoint.SendSystemError(e, SharedSettings.AppName);
                logger.LogError(e, "Unhandled exception occured");
            }
        }

        TimeSpan waitTs = TimeSpan.FromSeconds(5);
        await Task.Delay(waitTs, stoppingToken);
    }
}