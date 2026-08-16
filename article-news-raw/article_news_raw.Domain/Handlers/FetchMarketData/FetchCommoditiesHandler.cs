using System.Globalization;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using article_news_raw.DataAccess.Models.Api;
using article_news_raw.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace article_news_raw.Domain.Handlers.FetchMarketData;

public class FetchCommoditiesHandler(
    ILogger<FetchCommoditiesHandler> logger,
    IAlphaVantageCommoditiesService alphaVantageCommoditiesService,
    ICommodityRepository commodityRepository)
    : IFetchMarketDataHandler
{
    public string FetcherName => "AlphavantageCommodities";

    public async Task<int> HandleFetchMarketData(CancellationToken token = default)
    {
        var commodityFunctionsList =
            new List<(string CommodityType, Func<CancellationToken, Task<AlphaVantageCommodity>> Func)>
            {
                (CommodityTypes.Gold, alphaVantageCommoditiesService.GetGoldHistory),
                (CommodityTypes.Silver, alphaVantageCommoditiesService.GetSilverHistory),
                (CommodityTypes.Brent, alphaVantageCommoditiesService.GetBrentCrudeOilHistory)
            };

        logger.LogInformation("Fetching monthly history for {NrCommodities} commodities from AlphaVantage",
            commodityFunctionsList.Count);

        var totalUpserted = 0;

        foreach (var (commodityType, fetch) in commodityFunctionsList)
        {
            var response = await fetch(token);

            // A rate limited call comes back as HTTP 200 with an {"Information": ...} body,
            // which deserializes to an object with everything null.
            var dataPoints = response?.Data;
            if (dataPoints is null || dataPoints.Count == 0)
            {
                logger.LogWarning("No {CommodityType} data points returned from AlphaVantage", commodityType);
                continue;
            }

            var commodities = MapToCommodities(commodityType, dataPoints);
            var upserted = await commodityRepository.UpsertCommodities(commodities, token);
            totalUpserted += upserted;

            logger.LogInformation(
                "Fetched {NrDataPoints} {CommodityType} data points ({Interval}, {Unit}) from AlphaVantage, skipped {NrSkipped}, upserted {NrUpserted}",
                dataPoints.Count, commodityType, response!.Interval, response.Unit,
                dataPoints.Count - commodities.Count, upserted);
        }

        return totalUpserted;
    }

    private List<Commodity> MapToCommodities(string commodityType, List<CommodityDataPoint> dataPoints)
    {
        var result = new List<Commodity>();

        foreach (var dataPoint in dataPoints)
        {
            // Alphavantage quotes its numbers and uses "." for a missing observation.
            if (!double.TryParse(dataPoint.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                continue;
            if (!DateOnly.TryParse(dataPoint.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                continue;

            result.Add(new Commodity
            {
                Date = date,
                CommodityType = commodityType,
                Value = value
            });
        }

        return result;
    }
}
