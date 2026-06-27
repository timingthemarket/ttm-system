using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

public record BoersDataMarket(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("countryId")] long CountryId,
    [property: JsonPropertyName("isIndex")] bool IsIndex,
    [property: JsonPropertyName("exchangeName")] string ExchangeName
);

public record BoersDataMarkets(
    [property: JsonPropertyName("markets")] IReadOnlyList<BoersDataMarket> Markets
);