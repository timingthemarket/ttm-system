using System.Collections.Specialized;
using article_news_raw.DataAccess.Extensions;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models.Api;
using TTM.Shared.Constants;

namespace article_news_raw.DataAccess.Services;

public class AlphaVantageEconomicIndicatorsService : IAlphaVantageEconomicIndicatorsService
{
    private const string MonthlyInterval = "monthly";

    private readonly HttpClient _httpClient;
    private readonly string apiKey;

    public AlphaVantageEconomicIndicatorsService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        apiKey = Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY")
                 ?? throw new Exception("No environmentvariable for 'ALPHAVANTAGE_API_KEY' was found");

        _httpClient.BaseAddress = new Uri("https://www.alphavantage.co");
        // The full history is a few hundred data points, so allow more time than the news calls.
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public Task<AlphaVantageEconomicIndicator> GetInflationHistory(CancellationToken token = default)
        => GetIndicator(EconomicIndicatorTypes.Inflation, null, token);

    public Task<AlphaVantageEconomicIndicator> GetFederalFundsRateHistory(CancellationToken token = default)
        => GetIndicator(EconomicIndicatorTypes.FederalFundsRate, MonthlyInterval, token);

    private async Task<AlphaVantageEconomicIndicator> GetIndicator(string function, string? interval, CancellationToken token)
    {
        var parameters = new NameValueCollection
        {
            { "function", function },
            { "datatype", "json" },
            { "apikey", apiKey }
        };

        // INFLATION is published annually only and takes no interval parameter.
        if (interval is not null)
        {
            parameters.Add("interval", interval);
        }

        return await _httpClient.GetJson<AlphaVantageEconomicIndicator>("/query", parameters, token: token);
    }
}
