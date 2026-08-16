using System.Collections.Specialized;
using article_news_raw.DataAccess.Extensions;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models.Api;

namespace article_news_raw.DataAccess.Services;

public class AlphaVantageCommoditiesService : IAlphaVantageCommoditiesService
{
    private const string Interval = "monthly";

    private readonly HttpClient _httpClient;
    private readonly string apiKey;

    public AlphaVantageCommoditiesService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        apiKey = Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY")
                 ?? throw new Exception("No environmentvariable for 'ALPHAVANTAGE_API_KEY' was found");

        _httpClient.BaseAddress = new Uri("https://www.alphavantage.co");
        // The full monthly history is a few hundred data points, so allow more time than the news calls.
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public Task<AlphaVantageCommodity> GetGoldHistory(CancellationToken token = default)
        => GetCommodity("GOLD_SILVER_HISTORY", "GOLD", token);

    public Task<AlphaVantageCommodity> GetSilverHistory(CancellationToken token = default)
        => GetCommodity("GOLD_SILVER_HISTORY", "SILVER", token);

    public Task<AlphaVantageCommodity> GetBrentCrudeOilHistory(CancellationToken token = default)
        => GetCommodity("BRENT", null, token);

    private async Task<AlphaVantageCommodity> GetCommodity(string function, string? symbol, CancellationToken token)
    {
        var parameters = new NameValueCollection
        {
            { "function", function },
            { "interval", Interval },
            // GOLD_SILVER_HISTORY defaults to csv, so json has to be asked for explicitly.
            { "datatype", "json" },
            { "apikey", apiKey }
        };

        if (symbol is not null)
        {
            parameters.Add("symbol", symbol);
        }

        return await _httpClient.GetJson<AlphaVantageCommodity>("/query", parameters, token: token);
    }
}
