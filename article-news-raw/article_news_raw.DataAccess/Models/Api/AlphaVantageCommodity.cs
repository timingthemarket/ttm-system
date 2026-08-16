using System.Text.Json.Serialization;

namespace article_news_raw.DataAccess.Models.Api;

public class CommodityDataPoint
{
    [JsonPropertyName("date")] public string Date { get; set; }

    /// <summary>
    /// Alphavantage returns the number quoted, and uses "." for a missing observation.
    /// </summary>
    [JsonPropertyName("value")] public string Value { get; set; }
}

public class AlphaVantageCommodity
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("interval")] public string Interval { get; set; }

    [JsonPropertyName("unit")] public string Unit { get; set; }

    [JsonPropertyName("data")] public List<CommodityDataPoint> Data { get; set; }
}
