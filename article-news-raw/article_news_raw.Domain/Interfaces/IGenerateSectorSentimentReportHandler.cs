namespace article_news_raw.Domain.Interfaces;

public interface IGenerateSectorSentimentReportHandler
{
    Task GenerateAndSendReport();
}
