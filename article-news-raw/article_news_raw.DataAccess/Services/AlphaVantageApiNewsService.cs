using System.Collections.Specialized;
using article_news_raw.DataAccess.Extensions;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models.Api;

namespace article_news_raw.DataAccess.Services;

public class AlphaVantageApiNewsService : IAlphaVantageApiNewsService
{
    private readonly HttpClient _httpClient;
    private readonly string apiKey;

    public AlphaVantageApiNewsService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        apiKey = Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY")
                 ?? throw new Exception("No environmentvariable for 'ALPHAVANTAGE_API_KEY' was found");

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://www.alphavantage.co");
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<AlphaVantageNewsArticle> GetManufacturingNews(int limit, DateTime timeFrom, DateTime? timeTo = null)
    {
        var timeFromStr = timeFrom.ToString("yyyyMMddThhmm");
        var timeToStr = timeTo?.ToString("yyyyMMddThhmm");

        var parameters = new NameValueCollection
        {
            { "function", "NEWS_SENTIMENT" },
            { "limit", $"{limit}" },
            { "time_from", $"{timeFromStr}" },
            { "apikey", apiKey },
            { "topics", "manufacturing" }
        };

        if (timeToStr is not null)
        {
            parameters.Add("time_to", timeToStr);
        }

        return await _httpClient.GetJson<AlphaVantageNewsArticle>("/query", parameters);
    }

    public async Task<AlphaVantageNewsArticle> GetFinancialMarketNews(int limit, DateTime timeFrom, DateTime? timeTo = null)
    {
        var timeFromStr = timeFrom.ToString("yyyyMMddThhmm");
        var timeToStr = timeTo?.ToString("yyyyMMddThhmm");

        var parameters = new NameValueCollection
        {
            { "function", "NEWS_SENTIMENT" },
            { "limit", $"{limit}" },
            { "time_from", $"{timeFromStr}" },
            { "apikey", apiKey },
            { "topics", "financial_markets" }
        };

        if (timeToStr is not null)
        {
            parameters.Add("time_to", timeToStr);
        }

        return await _httpClient.GetJson<AlphaVantageNewsArticle>("/query", parameters);
    }

    public async Task<AlphaVantageNewsArticle> GetFinanceNews(int limit, DateTime timeFrom, DateTime? timeTo = null)
    {

        var timeFromStr = timeFrom.ToString("yyyyMMddThhmm");
        var timeToStr = timeTo?.ToString("yyyyMMddThhmm");

        var parameters = new NameValueCollection
        {
            { "function", "NEWS_SENTIMENT" },
            { "limit", $"{limit}" },
            { "time_from", $"{timeFromStr}" },
            { "apikey", apiKey },
            { "topics", "finance" }
        };

        if (timeToStr is not null)
        {
            parameters.Add("time_to", timeToStr);
        }

        return await _httpClient.GetJson<AlphaVantageNewsArticle>("/query", parameters);
    }

    public async Task<AlphaVantageNewsArticle> GetMacroEconomyNews(int limit, DateTime timeFrom, DateTime? timeTo = null)
    {
        var timeFromStr = timeFrom.ToString("yyyyMMddThhmm");
        var timeToStr = timeTo?.ToString("yyyyMMddThhmm");

        var parameters = new NameValueCollection
        {
            { "function", "NEWS_SENTIMENT" },
            { "limit", $"{limit}" },
            { "time_from", $"{timeFromStr}" },
            { "apikey", apiKey },
            { "topics", "economy_macro" }
        };

        if (timeToStr is not null)
        {
            parameters.Add("time_to", timeToStr);
        }

        return await _httpClient.GetJson<AlphaVantageNewsArticle>("/query", parameters);
    }

    public async Task<AlphaVantageNewsArticle> GetTechnologyNews(int limit, DateTime timeFrom, DateTime? timeTo = null)
    {
        var timeFromStr = timeFrom.ToString("yyyyMMddThhmm");
        var timeToStr = timeTo?.ToString("yyyyMMddThhmm");

        var parameters = new NameValueCollection
        {
            { "function", "NEWS_SENTIMENT" },
            { "limit", $"{limit}" },
            { "time_from", $"{timeFromStr}" },
            { "apikey", apiKey },
            { "topics", "technology" }
        };

        if (timeToStr is not null)
        {
            parameters.Add("time_to", timeToStr);
        }

        return await _httpClient.GetJson<AlphaVantageNewsArticle>("/query", parameters);
    }

    public async Task<AlphaVantageNewsArticle> GetRetailWholesaleNews(int limit, DateTime timeFrom, DateTime? timeTo = null)
    {
        var timeFromStr = timeFrom.ToString("yyyyMMddThhmm");
        var timeToStr = timeTo?.ToString("yyyyMMddThhmm");

        var parameters = new NameValueCollection
        {
            { "function", "NEWS_SENTIMENT" },
            { "limit", $"{limit}" },
            { "time_from", $"{timeFromStr}" },
            { "apikey", apiKey },
            { "topics", "retail_wholesale" }
        };

        if (timeToStr is not null)
        {
            parameters.Add("time_to", timeToStr);
        }

        return await _httpClient.GetJson<AlphaVantageNewsArticle>("/query", parameters);
    }

    public async Task<AlphaVantageNewsArticle> GetEarningsNews(int limit, DateTime timeFrom, DateTime? timeTo = null)
    {
        var timeFromStr = timeFrom.ToString("yyyyMMddThhmm");
        var timeToStr = timeTo?.ToString("yyyyMMddThhmm");

        var parameters = new NameValueCollection
        {
            { "function", "NEWS_SENTIMENT" },
            { "limit", $"{limit}" },
            { "time_from", $"{timeFromStr}" },
            { "apikey", apiKey },
            { "topics", "earnings" }
        };

        if (timeToStr is not null)
        {
            parameters.Add("time_to", timeToStr);
        }

        return await _httpClient.GetJson<AlphaVantageNewsArticle>("/query", parameters);
    }
}
