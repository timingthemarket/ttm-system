using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

public record BoersDataIndustry(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("sectorId")] long SectorId
);

public record BoersDataIndustries(
    [property: JsonPropertyName("branches")] IReadOnlyList<BoersDataIndustry> Branches
);