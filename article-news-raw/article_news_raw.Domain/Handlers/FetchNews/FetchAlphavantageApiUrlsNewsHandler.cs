using System.Globalization;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using article_news_raw.DataAccess.Models.Api;
using article_news_raw.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using TTM.Shared.Extensions;

namespace article_news_raw.Domain.Handlers.FetchNews;

public class FetchAlphavantageApiUrlsNewsHandler(
    ILogger<FetchAlphavantageApiUrlsNewsHandler> logger,
    IAlphaVantageApiNewsService alphaVantageApiNewsService,
    IArticleUrlRepository articleUrlRepository)
    : IFetchNewsUrlsHandler
{
    public string FetcherName => "Alphavantage";

    public async Task HandleFetchNewsUrls(DateTime? toDate = null)
    {
        var date = toDate?.AddDays(-5) ?? DateTime.UtcNow.AddDays(-1);
        logger.LogInformation("Fetching news urls from AlphaVantage date {Date}", date);
        
        var newsFunctionsList = new List<(Func<int, DateTime, DateTime?, Task<AlphaVantageNewsArticle>> Func, int Limit, string FuncName)>
        {
            (alphaVantageApiNewsService.GetFinanceNews, 150, "GetFinanceNews"),
            (alphaVantageApiNewsService.GetManufacturingNews, 100, "GetManufacturingNews"),
            (alphaVantageApiNewsService.GetFinancialMarketNews, 100, "GetFinancialMarketNews"),
            (alphaVantageApiNewsService.GetMacroEconomyNews, 150, "GetMacroEconomyNews"),
            (alphaVantageApiNewsService.GetEarningsNews, 150, "GetEarningsNews"),
            (alphaVantageApiNewsService.GetRetailWholesaleNews, 150, "GetRetailWholesaleNews"),
            (alphaVantageApiNewsService.GetTechnologyNews, 150, "GetTechnologyNews"),
        };

        var secondsWait = GetSecondsForWaitingBetweenCalls(newsFunctionsList.Count);
        
        foreach (var alphaNews in newsFunctionsList)
        {
            var news = await alphaNews.Func(alphaNews.Limit, date, toDate);
            var feed = news.Feed ?? new List<Feed>();
            if (feed.Count == 0)
            {
                logger.LogInformation("No articles to after mapping from AlphaVantage ({FName})", alphaNews.FuncName);
                continue;
            }

            var inserted = 0;
            foreach (var article in feed)
            {
                var articleUrl = MapToArticleUrl(article);
                var exists = await articleUrlRepository.ArticleSaved(article.Url);
                if (exists) continue;
                inserted++;
                await articleUrlRepository.SaveArticle(articleUrl);
            }
            
            logger.LogInformation("Fetched {NrArticles} articles from AlphaVantage with and inserted {Inserted} ({FName})", feed.Count, inserted, alphaNews.FuncName);

            if (secondsWait > 0)
                await Task.Delay(TimeSpan.FromSeconds(secondsWait));
        }
    }

    private int GetSecondsForWaitingBetweenCalls(int nrOperations)
    {
        const int maxCallsPerMinute = 5;
        if (nrOperations <= maxCallsPerMinute)
            return 0;

        var secondsMinimumWait = 60.0 / maxCallsPerMinute;
        return (int)Math.Ceiling(secondsMinimumWait);
    }

    private ArticleUrl MapToArticleUrl(Feed feed) =>
        new()
        {
            Url = feed.Url,
            DateArticlePublished =
                DateTime.ParseExact(feed.TimePublished, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture).ToUniversalTime(),
            DateUrlFetched = DateTime.UtcNow,
            TickerSentiments = MapToTickerSentiments(feed.TickerSentiment)
        };

    private List<ArticleTickerSentiment> MapToTickerSentiments(List<TickerSentiment>? sentiments)
    {
        var result = new List<ArticleTickerSentiment>();
        if (sentiments == null)
            return result;

        foreach (var sentiment in sentiments)
        {
            if (!double.TryParse(sentiment.TickerSentimentScore, NumberStyles.Any, CultureInfo.InvariantCulture, out var sentimentScore))
                continue;
            if (!double.TryParse(sentiment.RelevanceScore, NumberStyles.Any, CultureInfo.InvariantCulture, out var relevanceScore))
                continue;

            result.Add(new ArticleTickerSentiment
            {
                Ticker = sentiment.Ticker,
                SentimentScore = sentimentScore,
                RelevanceScore = relevanceScore
            });
        }

        return result;
    }
}