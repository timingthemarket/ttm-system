using TTM.Shared.Models.ArticleNewsRaw;

namespace article_news_raw.Domain.Interfaces;

public interface IQryArticleNewsSentimentHandler
{
    Task<List<SecurityNewsSentimentDto>> HandleGetTickerNewsSentiments(List<string> tickers, DateTime? from, DateTime? to);
}
