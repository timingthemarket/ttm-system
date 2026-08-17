using System.Text.Json.Serialization;

namespace article_news_raw.DataAccess.Models.Api;

public class EconomicIndicatorDataPoint
{
    [JsonPropertyName("date")] public string Date { get; set; }

    /// <summary>
    /// Alphavantage returns the number quoted, and uses "." for a missing observation.
    /// </summary>
    [JsonPropertyName("value")] public string Value { get; set; }
}

public class AlphaVantageEconomicIndicator
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("interval")] public string Interval { get; set; }

    [JsonPropertyName("unit")] public string Unit { get; set; }

    [JsonPropertyName("data")] public List<EconomicIndicatorDataPoint> Data { get; set; }
}
