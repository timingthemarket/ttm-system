using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

public record BoersDataCountry(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name
);

public record BoersDataCountries(
    [property: JsonPropertyName("countries")] IReadOnlyList<BoersDataCountry> Countries
);
