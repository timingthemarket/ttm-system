using article_news_raw.Domain.Constants;
using article_news_raw.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using TTM.Shared.Extensions;

namespace article_news_raw.Domain.Handlers;

public class FetchMarketDataHandler(
    ILogger<FetchMarketDataHandler> logger,
    IEnumerable<IFetchMarketDataHandler> fetchMarketDataHandlers,
    IPublishEndpoint publishEndpoint)
{
    public async Task FetchMarketData(CancellationToken token = default)
    {
        logger.LogInformation("Running market data fetch");

        var totalDataPoints = 0;

        foreach (var fetch in fetchMarketDataHandlers)
            try
            {
                var nrDataPoints = await fetch.HandleFetchMarketData(token);
                totalDataPoints += nrDataPoints;

                logger.LogInformation("Market data fetch from {Name} complete, stored {NrDataPoints} data points",
                    fetch.FetcherName, nrDataPoints);

                await publishEndpoint.Increment(Metrics.MARKET_DATA_FETCHED);
            }
            catch (Exception e)
            {
                await publishEndpoint.SendSystemError(e, nameof(article_news_raw));
            }

        logger.LogInformation("Market data fetch done, stored {TotalDataPoints} data points from {NrSources} sources",
            totalDataPoints, fetchMarketDataHandlers.Count());
    }
}
