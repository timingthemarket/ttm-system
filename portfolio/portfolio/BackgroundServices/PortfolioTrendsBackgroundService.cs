using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using portfolio.Domain.Interfaces;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;

namespace portfolio.BackgroundServices;

public class PortfolioTrendsBackgroundService : BackgroundService
{
    private readonly ILogger<PortfolioTrendsBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private bool _hasRunFirstTime = false;
    private DateTime _nextRunTime;
    private const int RestartDelaySeconds = 30;
    
    public PortfolioTrendsBackgroundService(
        ILogger<PortfolioTrendsBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        // Set the initial run time to the next Saturday at 3:00 AM
        _nextRunTime = GetNextSaturdayAt3AM();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        
        // Outer loop for restart functionality
        while (true)
        {
            try
            {
                _hasRunFirstTime = false; // Reset for each restart
                _nextRunTime = GetNextSaturdayAt3AM(); // Reset next run time
                
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ExecuteWorkCycle(stoppingToken);
                }
                
                // If we reach here, cancellation was requested
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("PortfolioTrendsBackgroundService cancellation requested (stoppingToken.IsCancellationRequested = true). Service will restart in {RestartDelay} seconds.", RestartDelaySeconds);
                    
                    // Wait before restarting
                    await Task.Delay(TimeSpan.FromSeconds(RestartDelaySeconds));
                    
                    _logger.LogInformation("Restarting PortfolioTrendsBackgroundService...");
                    continue; // Restart the service
                }
                
                break; // Normal shutdown
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("PortfolioTrendsBackgroundService received OperationCanceledException. Service will restart in {RestartDelay} seconds.", RestartDelaySeconds);
                
                // Wait before restarting
                await Task.Delay(TimeSpan.FromSeconds(RestartDelaySeconds));
                
                _logger.LogInformation("Restarting PortfolioTrendsBackgroundService after OperationCanceledException...");
                continue; // Restart the service
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in PortfolioTrendsBackgroundService. Service will restart in {RestartDelay} seconds.", RestartDelaySeconds);
                
                using var scope = _serviceProvider.CreateScope();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                await publishEndpoint.SendSystemError(ex, SharedSettings.AppName);
                
                // Wait before restarting
                await Task.Delay(TimeSpan.FromSeconds(RestartDelaySeconds));
                
                _logger.LogInformation("Restarting PortfolioTrendsBackgroundService after unexpected error...");
                continue; // Restart the service
            }
        }
    }

    private async Task ExecuteWorkCycle(CancellationToken stoppingToken)
    {
        if (!_hasRunFirstTime)
        {
            _logger.LogInformation("Starting Portfolio Trends Background Service...");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            _hasRunFirstTime = true;
            
            _logger.LogInformation("Next portfolio trends processing scheduled for {NextRunTime}", _nextRunTime);
        }

        // Check if it's time to run the service
        if (DateTime.UtcNow >= _nextRunTime)
        {
            await ProcessPortfolioTrends();
            UpdateNextRunTime();
        }
        
        // Check every minute for the next run time
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }

    private async Task ProcessPortfolioTrends()
    {
        using var scope = _serviceProvider.CreateScope();
        var portfolioTrendsHandler = scope.ServiceProvider.GetRequiredService<IPortfolioTrendsHandler>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
        try
        {
            _logger.LogInformation("Processing portfolio trends...");
            
            await portfolioTrendsHandler.ProcessPortfolioTrends();
            
            _logger.LogInformation("Portfolio trends processing completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process portfolio trends: {ErrorMessage}", ex.Message);
            await publishEndpoint.SendSystemError(ex, SharedSettings.AppName);
        }
    }

    private void UpdateNextRunTime()
    {
        // Set the next run time to the next Saturday at 3:00 AM
        _nextRunTime = GetNextSaturdayAt3AM();
        _logger.LogInformation("Next portfolio trends processing scheduled for {NextRunTime}", _nextRunTime);
    }

    private static DateTime GetNextSaturdayAt3AM()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        
        // Calculate days until next Saturday
        int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
        
        // If today is Saturday and it's before 3 AM, run today at 3 AM
        if (daysUntilSaturday == 0 && now.Hour < 3)
        {
            return today.AddHours(3);
        }
        
        // If daysUntilSaturday is 0, it means today is Saturday but after 3 AM, so go to next Saturday
        if (daysUntilSaturday == 0)
        {
            daysUntilSaturday = 7;
        }
        
        var nextSaturday = today.AddDays(daysUntilSaturday);
        return nextSaturday.AddHours(3); // 3:00 AM UTC
    }
}