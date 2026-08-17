using System.Globalization;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using article_news_raw.DataAccess.Models.Api;
using article_news_raw.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using TTM.Shared.Constants;

namespace article_news_raw.Domain.Handlers.FetchMarketData;

public class FetchIndexDataHandler(
    ILogger<FetchIndexDataHandler> logger,
    IAlphaVantageIndexDataService alphaVantageIndexDataService,
    IIndexDataRepository indexDataRepository)
    : IFetchMarketDataHandler
{
    public string FetcherName => "AlphavantageIndexData";

    public async Task<int> HandleFetchMarketData(CancellationToken token = default)
    {
        var indexFunctionsList =
            new List<(string IndexType, Func<CancellationToken, Task<AlphaVantageIndex>> Func)>
            {
                (IndexTypes.Sp500, alphaVantageIndexDataService.GetSp500History),
                (IndexTypes.Vix, alphaVantageIndexDataService.GetVixHistory)
            };

        logger.LogInformation("Fetching daily history for {NrIndexes} indexes from AlphaVantage",
            indexFunctionsList.Count);

        var totalUpserted = 0;

        foreach (var (indexType, fetch) in indexFunctionsList)
        {
            var response = await fetch(token);

            // A rate limited call comes back as HTTP 200 with an {"Information": ...} body,
            // which deserializes to an object with everything null.
            var dataPoints = response?.Data;
            if (dataPoints is null || dataPoints.Count == 0)
            {
                logger.LogWarning("No {IndexType} data points returned from AlphaVantage", indexType);
                continue;
            }

            var indexData = MapToIndexData(indexType, dataPoints);
            var upserted = await indexDataRepository.UpsertIndexData(indexData, token);
            totalUpserted += upserted;

            logger.LogInformation(
                "Fetched {NrDataPoints} {IndexType} data points ({Interval}) from AlphaVantage, skipped {NrSkipped}, upserted {NrUpserted}",
                dataPoints.Count, indexType, response!.Interval,
                dataPoints.Count - indexData.Count, upserted);
        }

        return totalUpserted;
    }

    private List<IndexData> MapToIndexData(string indexType, List<IndexDataPoint> dataPoints)
    {
        var result = new List<IndexData>();

        foreach (var dataPoint in dataPoints)
        {
            // Alphavantage quotes its numbers. We keep the close of each observation as the value.
            if (!double.TryParse(dataPoint.Close, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                continue;
            if (!DateOnly.TryParse(dataPoint.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                continue;

            result.Add(new IndexData
            {
                Date = date,
                IndexType = indexType,
                Value = value
            });
        }

        return result;
    }
}
