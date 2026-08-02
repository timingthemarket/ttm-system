using article_news_raw.DataAccess.InternalEvents;
using article_news_raw.Domain.Interfaces;
using MassTransit;

namespace article_news_raw.Triggers;

public class SectorSentimentReportTrigger(IGenerateSectorSentimentReportHandler generateSectorSentimentReportHandler)
    : IConsumer<GenerateSectorSentimentReportTriggerEvent>
{
    public async Task Consume(ConsumeContext<GenerateSectorSentimentReportTriggerEvent> context)
    {
        await generateSectorSentimentReportHandler.GenerateAndSendReport();
    }
}
