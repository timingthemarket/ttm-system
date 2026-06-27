using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

public record BoersDataStockPrice(
    [property: JsonPropertyName("d")] DateTime Date,
    [property: JsonPropertyName("h")] double? High,
    [property: JsonPropertyName("l")] double? Low,
    [property: JsonPropertyName("c")] double? Close,
    [property: JsonPropertyName("o")] double? Open,
    [property: JsonPropertyName("v")] long? Volume
);

public record BoersDataStockPriceArray(
    [property: JsonPropertyName("instrument")] long Instrument,
    [property: JsonPropertyName("stockPricesList")] IReadOnlyList<BoersDataStockPrice> StockPricesList,
    [property: JsonPropertyName("error")] string Error
);

public record BoersDataStockPrices(
    [property: JsonPropertyName("stockPricesArrayList")] IReadOnlyList<BoersDataStockPriceArray> StockPricesArrayList
);