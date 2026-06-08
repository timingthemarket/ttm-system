using article_news_raw.Domain.Constants;
using article_news_raw.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using TTM.Shared.Extensions;

namespace article_news_raw.Domain.Handlers;

public class FetchNewsUrlsHandler(
    ILogger<FetchNewsUrlsHandler> logger,
    IEnumerable<IFetchNewsUrlsHandler> fetchNewsHandlers,
    IPublishEndpoint publishEndpoint)
{
    public async Task FetchNewsUrls(DateTime? toDate = null)
    {
        logger.LogInformation("Running news fetch");

        foreach (var fetch in fetchNewsHandlers)
            try
            {
                await fetch.HandleFetchNewsUrls(toDate);
                logger.LogInformation("News fetch from {Name} complete", fetch.FetcherName);
                await publishEndpoint.Increment(Metrics.ARTICLE_URL_FETCHED);
            }
            catch (Exception e)
            {
                await publishEndpoint.SendSystemError(e, nameof(article_news_raw));
            }
    }
}
