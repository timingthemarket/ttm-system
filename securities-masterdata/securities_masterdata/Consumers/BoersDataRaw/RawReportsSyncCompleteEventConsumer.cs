using MassTransit;
using securities_masterdata.Services;
using TTM.Shared.Events.BoersDataRaw;

namespace securities_masterdata.Consumers.BoersDataRaw;

public class RawReportsSyncCompleteEventConsumer(IBackfillQueueService queueService, ILogger<RawReportsSyncCompleteEventConsumer> logger)
    : IConsumer<RawReportsSyncCompleteEvent>
{
    public Task Consume(ConsumeContext<RawReportsSyncCompleteEvent> context)
    {
        logger.LogInformation("Received RawReportsSyncCompleteEvent, enqueueing backfill request");
        queueService.EnqueueBackfillRequest();
        return Task.CompletedTask;
    }
}