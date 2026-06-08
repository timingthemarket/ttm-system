using System.Text.Json.Serialization;

namespace article_news_raw.DataAccess.Models.Api;

public record FinnHubNewsArticle
{
    [JsonPropertyName("category")] public string Category { get; set; }

    [JsonPropertyName("datetime")] public long Datetime { get; set; }

    [JsonPropertyName("headline")] public string Headline { get; set; }

    [JsonPropertyName("id")] public long Id { get; set; }

    [JsonPropertyName("image")] public string Image { get; set; }

    [JsonPropertyName("related")] public string Related { get; set; }

    [JsonPropertyName("source")] public string Source { get; set; }

    [JsonPropertyName("summary")] public string Summary { get; set; }

    [JsonPropertyName("url")] public string Url { get; set; }
}