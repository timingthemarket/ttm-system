using System.Diagnostics.CodeAnalysis;
using boersdata_raw.Domain.Models;
using boersdata_raw.Domain.Queue;
using Hangfire;

namespace boersdata_raw.Scheduler;

[ExcludeFromCodeCoverage]
public class SetupHangfireJobs
{
    private readonly IQueueCache<DailyPricesQueue> _dailyPricesQueue;
    private readonly IQueueCache<ReportsQueue> _reportsQueue;
    private readonly IQueueCache<WeeklyRefreshPricesQueue> _weeklyRefreshPricesQueue;
    
    /// <summary>
    /// </summary>
    /// <param name="publishEndpoint"></param>
    /// <param name="backgroundJobClient">
    ///     DONT REMOVE THIS - It is nessecary to instanciate to be able to add jobs to the
    ///     scheduler
    /// </param>
    public SetupHangfireJobs(
        IQueueCache<DailyPricesQueue> dailyPricesQueue, 
        IQueueCache<ReportsQueue> reportsQueue,
        IQueueCache<WeeklyRefreshPricesQueue> weeklyRefreshPricesQueue,
        IBackgroundJobClient backgroundJobClient)
    {
        _dailyPricesQueue = dailyPricesQueue;
        _reportsQueue = reportsQueue;
        _weeklyRefreshPricesQueue = weeklyRefreshPricesQueue;
    }

    public void SetupJobs()
    {
        RecurringJob.AddOrUpdate(
            "weekly-report-sync",
            () => PublishReportsWeeklySyncEvent(),
            Cron.Weekly(DayOfWeek.Sunday, 9),
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
        
        RecurringJob.AddOrUpdate(
            "weekly-refresh-prices-sync",
            () => PublishWeeklyRefreshPricesSyncEvent(),
            Cron.Weekly(DayOfWeek.Saturday, 18),
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        /*RecurringJob.AddOrUpdate(
            "daily-price-sync",
            () => PublishDailyPriceSyncEvent(),
            "30 21,3 * * 1-5",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });*/
    }

    public void PublishDailyPriceSyncEvent() => _dailyPricesQueue.Enqueue(new DailyPricesQueue());
    public void PublishReportsWeeklySyncEvent() => _reportsQueue.Enqueue(new ReportsQueue());
    public void PublishWeeklyRefreshPricesSyncEvent() => _weeklyRefreshPricesQueue.Enqueue(new WeeklyRefreshPricesQueue());

}