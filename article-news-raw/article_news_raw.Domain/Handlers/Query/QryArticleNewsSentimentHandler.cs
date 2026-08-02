using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using article_news_raw.Domain.Interfaces;
using TTM.Shared.Models.ArticleNewsRaw;

namespace article_news_raw.Domain.Handlers.Query;

public class QryArticleNewsSentimentHandler(IArticleUrlRepository articleUrlRepository) : IQryArticleNewsSentimentHandler
{
    public async Task<List<SecurityNewsSentimentDto>> HandleGetTickerNewsSentiments(List<string> tickers, DateTime? from, DateTime? to)
    {
        var sentiments = await articleUrlRepository.GetTickerSentiments(tickers, from, to);
        return MapToSecurityNewsSentimentDtos(sentiments);
    }

    private static List<SecurityNewsSentimentDto> MapToSecurityNewsSentimentDtos(List<ArticleTickerSentiment> sentiments) =>
        sentiments
            .GroupBy(s => s.Ticker)
            .Select(g => new SecurityNewsSentimentDto
            {
                Ticker = g.Key,
                NrOccurances = g.Count(),
                AverageSentiment = g.Average(s => s.SentimentScore)
            })
            .ToList();
}
