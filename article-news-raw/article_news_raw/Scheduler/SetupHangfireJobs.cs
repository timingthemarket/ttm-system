using System.Diagnostics.CodeAnalysis;
using article_news_raw.DataAccess.InternalEvents;
using Hangfire;
using MassTransit;

namespace article_news_raw.Scheduler;

[ExcludeFromCodeCoverage]
public class SetupHangfireJobs
{
    private readonly IPublishEndpoint _publishEndpoint;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="publishEndpoint"></param>
    /// <param name="backgroundJobClient">DONT REMOVE THIS - It is nessecary to instanciate to be able to add jobs to the scheduler</param>
    public SetupHangfireJobs(IPublishEndpoint publishEndpoint, IBackgroundJobClient backgroundJobClient)
    {
        _publishEndpoint = publishEndpoint;
    }

    public void SetupJobs()
    {
        RecurringJob.AddOrUpdate(
            "daily-news-url-sync",
            () => PublishFetchNewesUrlsEvent(),
            "*/30 * * * *", // "20 3,9,15,21 * * *",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc,
                MisfireHandling = MisfireHandlingMode.Strict
            });

        RecurringJob.AddOrUpdate(
            "weekly-market-data-sync",
            () => PublishFetchMarketDataEvent(),
            "0 5 * * 1", // Mondays 05:00 UTC
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc,
                MisfireHandling = MisfireHandlingMode.Strict
            });

        RecurringJob.AddOrUpdate(
            "weekly-sector-sentiment-report",
            () => PublishGenerateSectorSentimentReportEvent(),
            "0 6 * * 6",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc,
                MisfireHandling = MisfireHandlingMode.Strict
            });
    }

    public async Task PublishFetchNewesUrlsEvent() => await _publishEndpoint.Publish(new FetchNewesUrlsTriggerEvent());

    public async Task PublishFetchMarketDataEvent() => await _publishEndpoint.Publish(new FetchMarketDataTriggerEvent());

    public async Task PublishGenerateSectorSentimentReportEvent() =>
        await _publishEndpoint.Publish(new GenerateSectorSentimentReportTriggerEvent());
}