using System.Collections.Specialized;
using article_news_raw.DataAccess.Extensions;
using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using article_news_raw.DataAccess.Models.Api;

namespace article_news_raw.DataAccess.Services;

public class FinnhubApiNewsService : IFinnhubApiNewsService
{
    private readonly HttpClient _httpClient;
    private long MaxId = 0;

    public FinnhubApiNewsService(HttpClient httpClient)
    {
        var token = Environment.GetEnvironmentVariable("FINNHUB_API_KEY") 
                    ?? throw new Exception("No environmentvariable for 'FINNHUB_API_KEY' was found");
        
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("X-Finnhub-Token", token);
        _httpClient.BaseAddress = new Uri("https://finnhub.io/api/v1");
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<List<FinnHubNewsArticle>> FetchArticles()
    {
        var parameters = new NameValueCollection
        {
            { "category", "general" },
        };
        if (MaxId > 0)
        {
            parameters["minId"] = MaxId.ToString();
        }

        var response = await _httpClient.GetJson<List<FinnHubNewsArticle>>("/news", parameters);
        var list = response ?? new List<FinnHubNewsArticle>();
            
        MaxId = list.Select(r => r.Id).Max();
        return list;
    }
}