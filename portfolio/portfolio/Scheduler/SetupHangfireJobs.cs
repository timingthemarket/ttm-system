using System.Diagnostics.CodeAnalysis;
using Hangfire;
using portfolio.Domain.Handlers;
using portfolio.Domain.Interfaces;

namespace portfolio.Scheduler;

/// <summary>
/// </summary>
/// <param name="publishEndpoint"></param>
/// <param name="backgroundJobClient">
///     DONT REMOVE THIS - It is nessecary to instanciate to be able to add jobs to the
///     scheduler
/// </param>
[ExcludeFromCodeCoverage]
public class SetupHangfireJobs(IBackgroundJobClient backgroundJobClient, IPortfolioExplorerNotificationService notificationService,
    SessionDateHandler sessionDateHandler)
{
    private readonly RecurringJobOptions _recurringJobOptions = new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Utc
    };
    
    public void SetupJobs()
    {
        // WORKER ON SCHEDULE
        // worker on saturday 06:00 UTC
        RecurringJob.AddOrUpdate(
            "worker-on-monday-saturday",
            () => ToggleSessionDate(),
            "* 6 * * 6",
            _recurringJobOptions);
        
        // notification for worker on monday 06:30
        /*RecurringJob.AddOrUpdate(
            "notification-on-monday",
            () => StartNotification(),
            "30 6 * * 1",
            _recurringJobOptions);*/
    }
    
    //public async Task StartNotification() => await notificationService.ProcessPortfolioExplorerNotification();
    public async Task ToggleSessionDate() => await sessionDateHandler.ToggleSessionDate();
}