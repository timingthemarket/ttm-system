using System.Globalization;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using article_news_raw.DataAccess.Models.Api;
using article_news_raw.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using TTM.Shared.Constants;

namespace article_news_raw.Domain.Handlers.FetchMarketData;

public class FetchEconomicIndicatorsHandler(
    ILogger<FetchEconomicIndicatorsHandler> logger,
    IAlphaVantageEconomicIndicatorsService alphaVantageEconomicIndicatorsService,
    IEconomicIndicatorRepository economicIndicatorRepository)
    : IFetchMarketDataHandler
{
    public string FetcherName => "AlphavantageEconomicIndicators";

    public async Task<int> HandleFetchMarketData(CancellationToken token = default)
    {
        var indicatorFunctionsList =
            new List<(string IndicatorType, Func<CancellationToken, Task<AlphaVantageEconomicIndicator>> Func)>
            {
                (EconomicIndicatorTypes.Inflation, alphaVantageEconomicIndicatorsService.GetInflationHistory),
                (EconomicIndicatorTypes.FederalFundsRate, alphaVantageEconomicIndicatorsService.GetFederalFundsRateHistory)
            };

        logger.LogInformation("Fetching history for {NrIndicators} economic indicators from AlphaVantage",
            indicatorFunctionsList.Count);

        var totalUpserted = 0;

        foreach (var (indicatorType, fetch) in indicatorFunctionsList)
        {
            var response = await fetch(token);

            // A rate limited call comes back as HTTP 200 with an {"Information": ...} body,
            // which deserializes to an object with everything null.
            var dataPoints = response?.Data;
            if (dataPoints is null || dataPoints.Count == 0)
            {
                logger.LogWarning("No {IndicatorType} data points returned from AlphaVantage", indicatorType);
                continue;
            }

            var economicIndicators = MapToEconomicIndicators(indicatorType, dataPoints);
            var upserted = await economicIndicatorRepository.UpsertEconomicIndicators(economicIndicators, token);
            totalUpserted += upserted;

            logger.LogInformation(
                "Fetched {NrDataPoints} {IndicatorType} data points ({Interval}, {Unit}) from AlphaVantage, skipped {NrSkipped}, upserted {NrUpserted}",
                dataPoints.Count, indicatorType, response!.Interval, response.Unit,
                dataPoints.Count - economicIndicators.Count, upserted);
        }

        return totalUpserted;
    }

    private List<EconomicIndicator> MapToEconomicIndicators(string indicatorType, List<EconomicIndicatorDataPoint> dataPoints)
    {
        var result = new List<EconomicIndicator>();

        foreach (var dataPoint in dataPoints)
        {
            // Alphavantage quotes its numbers, and uses "." for a missing observation.
            if (!double.TryParse(dataPoint.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                continue;
            if (!DateOnly.TryParse(dataPoint.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                continue;

            result.Add(new EconomicIndicator
            {
                Date = date,
                IndicatorType = indicatorType,
                Value = value
            });
        }

        return result;
    }
}
