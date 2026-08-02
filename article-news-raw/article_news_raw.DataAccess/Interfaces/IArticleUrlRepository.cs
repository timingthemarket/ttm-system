using article_news_raw.DataAccess.Models;

namespace article_news_raw.DataAccess.Interfaces;

public interface IArticleUrlRepository
{
    Task<int> SaveBatch(List<ArticleUrl> urls, CancellationToken token = default);
    Task<bool> ArticleSaved(string url, CancellationToken token = default);
    Task<int> SaveArticle(ArticleUrl url, CancellationToken token = default);
    Task<List<ArticleTickerSentiment>> GetTickerSentiments(List<string> tickers, DateTime? from, DateTime? to, CancellationToken token = default);
}