using System.Text.Json.Serialization;

namespace article_news_raw.DataAccess.Models.Api;

public class IndexDataPoint
{
    [JsonPropertyName("date")] public string Date { get; set; }

    [JsonPropertyName("open")] public string Open { get; set; }

    [JsonPropertyName("high")] public string High { get; set; }

    [JsonPropertyName("low")] public string Low { get; set; }

    /// <summary>
    /// The observation we persist. Alphavantage returns the number quoted.
    /// </summary>
    [JsonPropertyName("close")] public string Close { get; set; }
}

public class AlphaVantageIndex
{
    [JsonPropertyName("symbol")] public string Symbol { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("interval")] public string Interval { get; set; }

    [JsonPropertyName("data")] public List<IndexDataPoint> Data { get; set; }
}
