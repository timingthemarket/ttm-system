using System.Text.Json.Serialization;

namespace article_news_raw.DataAccess.Models.Api;

public class TickerSentiment
{
    [JsonPropertyName("ticker")] public string Ticker { get; set; }
    [JsonPropertyName("relevance_score")] public string RelevanceScore { get; set; }
    [JsonPropertyName("ticker_sentiment_score")]
    public string TickerSentimentScore { get; set; }
}

public class Feed
{
    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("url")] public string Url { get; set; }

    [JsonPropertyName("time_published")] public string TimePublished { get; set; }

    [JsonPropertyName("summary")] public string Summary { get; set; }
    
    [JsonPropertyName("source")] public string? Source { get; set; }
    
    [JsonPropertyName("source_domain")] public string SourceDomain { get; set; }
    [JsonPropertyName("ticker_sentiment")] public List<TickerSentiment> TickerSentiment { get; set; }
}

public class AlphaVantageNewsArticle
{
    [JsonPropertyName("items")] public string NrItems { get; set; }

    [JsonPropertyName("feed")] public List<Feed> Feed { get; set; }
}