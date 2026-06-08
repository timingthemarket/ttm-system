using System.Text.Json.Serialization;

namespace securities_masterdata.DataAccess.Services.Models;

public class AvanzaStockFilterResponse
{
    [JsonPropertyName("stocks")]
    public AvanzaStock[] Stocks { get; set; } = [];
}


public class AvanzaStock
{
    [JsonPropertyName("orderbookId")]
    public string OrderbookId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")] public string Type { get; set; } = null!;

    [JsonPropertyName("shortName")] public string Ticker { get; set; } = null!;
}