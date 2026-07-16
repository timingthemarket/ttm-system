using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using portfolio.Domain.Interfaces;
using portfolio.DataAccess;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;

namespace portfolio.BackgroundServices;

/*
    CREATE MATERIALIZED VIEW portfolio_outcome_view AS
    SELECT 
		    ROW_NUMBER() OVER (ORDER BY s.session_date, p.id) AS id,
        s.session_date,
        MD5(
            CONCAT(
                STRING_AGG(
                    CONCAT(
                        pi.indicator::text, '|',
                        pi.weight::text, '|', 
                        pi.direction::text, '|',
                        pi.lookback, '|'
                    ), 
                    ','
                    ORDER BY pi.indicator, pi.weight, pi.direction, pi.lookback
                )
            )
        ) AS set_id,
        sim.percentage_change
    FROM session s
    INNER JOIN simulation sim ON s.id = sim.session_id
    INNER JOIN simulation_period sp ON sim.id = sp.simulation_id
    INNER JOIN portfolio p ON sp.portfolio_id = p.id
    INNER JOIN portfolio_indicators pi ON p.id = pi.portfolio_id
    GROUP BY s.session_date, sim.percentage_change, p.id
    ORDER BY s.session_date, p.id

    CREATE UNIQUE INDEX ON portfolio_outcome_view (id);
 */
public class PortfolioOutcomeViewRefreshBackgroundService : BackgroundService
{
    private readonly ILogger<PortfolioOutcomeViewRefreshBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private bool _hasRunFirstTime = false;
    private DateTime _nextRunTime;
    private const int RestartDelaySeconds = 30;
    
    public PortfolioOutcomeViewRefreshBackgroundService(
        ILogger<PortfolioOutcomeViewRefreshBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        // Set the initial run time to the next day at 2:00 AM
        _nextRunTime = DateTime.UtcNow.Date.AddDays(1).AddHours(2);
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
                _nextRunTime = DateTime.UtcNow.Date.AddDays(1).AddHours(2); // Reset next run time
                
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ExecuteWorkCycle(stoppingToken);
                }
                
                // If we reach here, cancellation was requested
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("PortfolioOutcomeViewRefreshBackgroundService cancellation requested (stoppingToken.IsCancellationRequested = true). Service will restart in {RestartDelay} seconds.", RestartDelaySeconds);
                    
                    // Wait before restarting
                    await Task.Delay(TimeSpan.FromSeconds(RestartDelaySeconds));
                    
                    _logger.LogInformation("Restarting PortfolioOutcomeViewRefreshBackgroundService...");
                    continue; // Restart the service
                }
                
                break; // Normal shutdown
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("PortfolioOutcomeViewRefreshBackgroundService received OperationCanceledException. Service will restart in {RestartDelay} seconds.", RestartDelaySeconds);
                
                // Wait before restarting
                await Task.Delay(TimeSpan.FromSeconds(RestartDelaySeconds));
                
                _logger.LogInformation("Restarting PortfolioOutcomeViewRefreshBackgroundService after OperationCanceledException...");
                continue; // Restart the service
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in PortfolioOutcomeViewRefreshBackgroundService. Service will restart in {RestartDelay} seconds.", RestartDelaySeconds);
                
                using var scope = _serviceProvider.CreateScope();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                await publishEndpoint.SendSystemError(ex, SharedSettings.AppName);
                
                // Wait before restarting
                await Task.Delay(TimeSpan.FromSeconds(RestartDelaySeconds));
                
                _logger.LogInformation("Restarting PortfolioOutcomeViewRefreshBackgroundService after unexpected error...");
                continue; // Restart the service
            }
        }
    }

    private async Task ExecuteWorkCycle(CancellationToken stoppingToken)
    {
        if (!_hasRunFirstTime)
        {
            _logger.LogInformation("Starting Portfolio outcome refresh Service...");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            _hasRunFirstTime = true;
            
            // Run the materialized view refresh on first startup
            await RefreshPortfolioOutcomeView();
            
            UpdateNextRunTime();
        }

        // Check if it's time to run the service
        if (DateTime.UtcNow >= _nextRunTime)
        {
            await RefreshPortfolioOutcomeView();
            UpdateNextRunTime();
        }
        
        // Check every minute for the next run time
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }

    private async Task RefreshPortfolioOutcomeView()
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
        try
        {
            _logger.LogInformation("Refreshing portfolio outcome materialized view...");
            
            // Set command timeout to 30 minutes for long-running materialized view refresh
            context.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));
            
            await context.Database.ExecuteSqlRawAsync("REFRESH MATERIALIZED VIEW portfolio_outcome_view;");
            
            _logger.LogInformation("Portfolio outcome materialized view refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh portfolio outcome materialized view: {ErrorMessage}", ex.Message);
            await publishEndpoint.SendSystemError(ex, SharedSettings.AppName);
        }
    }

    private void UpdateNextRunTime()
    {
        // Set the next run time to the next day at 2:00 AM
        _nextRunTime = DateTime.UtcNow.Date.AddDays(1).AddHours(2);
        _logger.LogInformation("Next portfolio outcome materialized view scheduled for {NextRunTime}", _nextRunTime);
    }
}
