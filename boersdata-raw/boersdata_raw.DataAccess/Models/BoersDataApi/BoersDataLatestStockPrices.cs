using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

public record BoersDataLatestStockPrice(
    [property: JsonPropertyName("i")] long InstrumentId,
    [property: JsonPropertyName("d")] DateTime Date,
    [property: JsonPropertyName("h")] double? High,
    [property: JsonPropertyName("l")] double? Low,
    [property: JsonPropertyName("c")] double? Close,
    [property: JsonPropertyName("o")] double? Open,
    [property: JsonPropertyName("v")] long? Volume
);

public record BoersDataLatestStockPrices(
    [property: JsonPropertyName("stockPricesList")] IReadOnlyList<BoersDataLatestStockPrice> StockPricesList
);

