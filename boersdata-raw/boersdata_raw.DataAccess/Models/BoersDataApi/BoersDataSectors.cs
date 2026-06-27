using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

public record BoersDataSector(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name
);

public record BoersDataSectors(
    [property: JsonPropertyName("sectors")] IReadOnlyList<BoersDataSector> Sectors
);