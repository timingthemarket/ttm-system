using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using article_news_raw.DataAccess.Models.Api;
using article_news_raw.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace article_news_raw.Domain.Handlers.FetchNews;

public class FetchFinnhubApiUrlNewsHandler(
    ILogger<FetchFinnhubApiUrlNewsHandler> logger,
    IFinnhubApiNewsService finnhubApiNewsService,
    IArticleUrlRepository articleUrlRepository)
    : IFetchNewsUrlsHandler
{
    public string FetcherName => "Finnhub";

    public async Task HandleFetchNewsUrls(DateTime? toDate = null)
    {
        logger.LogInformation("Fetching news urls from Finnhub");

        var finnhubArticleUrls = await finnhubApiNewsService.FetchArticles();
        var articleUrls = MapToArticleUrl(finnhubArticleUrls);
        if (!articleUrls.Any())
        {
            logger.LogInformation("No articles to after mapping from FinnHub");
            return;
        }
        
        logger.LogInformation("Fetched {NrArticles} articles from Finnhub", articleUrls.Count);

        await articleUrlRepository.SaveBatch(articleUrls);
    }

    private List<ArticleUrl> MapToArticleUrl(List<FinnHubNewsArticle> articleUrls) =>
        articleUrls.Select(a => new ArticleUrl
        {
            Url = a.Url,
            DateArticlePublished = DateTimeOffset.FromUnixTimeSeconds(a.Datetime).UtcDateTime,
            DateUrlFetched = DateTime.UtcNow
        }).ToList();
}