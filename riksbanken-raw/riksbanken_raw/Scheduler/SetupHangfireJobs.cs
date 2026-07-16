using System.Diagnostics.CodeAnalysis;
using Hangfire;
using MassTransit;
using riksbanken_raw.Domain.Events;

namespace riksbanken_raw.Scheduler;

[ExcludeFromCodeCoverage]
public class SetupHangfireJobs
{
    private readonly IPublishEndpoint _publishEndpoint;

    /// <summary>
    /// </summary>
    /// <param name="publishEndpoint"></param>
    /// <param name="backgroundJobClient">
    ///     DONT REMOVE THIS - It is nessecary to instanciate to be able to add jobs to the
    ///     scheduler
    /// </param>
    public SetupHangfireJobs(IPublishEndpoint publishEndpoint, IBackgroundJobClient backgroundJobClient) =>
        _publishEndpoint = publishEndpoint;

    public void SetupJobs()
    {
        RecurringJob.AddOrUpdate(
            "daily-currencyrates-sync",
            () => PublishDailyPriceSyncEvent(),
            "30 1/3 * * 1-5",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc,
                MisfireHandling = MisfireHandlingMode.Strict
            });
    }

    public async Task PublishDailyPriceSyncEvent() =>
        await _publishEndpoint.Publish(new SyncDailyCurrencyTriggerEvent(), c => c.CorrelationId = NewId.NextGuid());
}