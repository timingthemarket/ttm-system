using System.Collections.Specialized;
using article_news_raw.DataAccess.Extensions;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using article_news_raw.DataAccess.Models.Api;
using TTM.Shared.Constants;

namespace article_news_raw.DataAccess.Services;

public class AlphaVantageIndexDataService : IAlphaVantageIndexDataService
{
    private const string Function = "INDEX_DATA";
    private const string Interval = "daily";

    private readonly HttpClient _httpClient;
    private readonly string apiKey;

    public AlphaVantageIndexDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        apiKey = Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY")
                 ?? throw new Exception("No environmentvariable for 'ALPHAVANTAGE_API_KEY' was found");

        _httpClient.BaseAddress = new Uri("https://www.alphavantage.co");
        // The full daily history is several thousand data points, so allow more time than the commodity calls.
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    public Task<AlphaVantageIndex> GetSp500History(CancellationToken token = default)
        => GetIndex(IndexTypes.Sp500, token);

    public Task<AlphaVantageIndex> GetVixHistory(CancellationToken token = default)
        => GetIndex(IndexTypes.Vix, token);

    private async Task<AlphaVantageIndex> GetIndex(string symbol, CancellationToken token)
    {
        var parameters = new NameValueCollection
        {
            { "function", Function },
            { "symbol", symbol },
            { "interval", Interval },
            { "datatype", "json" },
            { "apikey", apiKey }
        };

        return await _httpClient.GetJson<AlphaVantageIndex>("/query", parameters, token: token);
    }
}
