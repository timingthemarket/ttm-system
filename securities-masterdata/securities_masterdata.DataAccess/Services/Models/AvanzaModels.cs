using System.Text.Json.Serialization;

namespace securities_masterdata.DataAccess.Services.Models;

public class AvanzaStockFilterRequest
{
    [JsonPropertyName("filter")]
    public AvanzaFilter Filter { get; set; } = new();

    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("sortBy")]
    public AvanzaSortBy SortBy { get; set; } = new();
}

public class AvanzaFilter
{
    [JsonPropertyName("sectors")]
    public string[] Sectors { get; set; } = Array.Empty<string>();

    [JsonPropertyName("marketPlaces")]
    public string[] MarketPlaces { get; set; } = Array.Empty<string>();
}

public class AvanzaSortBy
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = "numberOfOwners";

    [JsonPropertyName("order")]
    public string Order { get; set; } = "desc";
}
