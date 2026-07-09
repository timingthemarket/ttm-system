using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

public record BoersDataInstrumentUpdate(
    [property: JsonPropertyName("insId")] int InsId,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt
);

public record BoersDataInstrumentsUpdates(
    [property: JsonPropertyName("instruments")] IReadOnlyList<BoersDataInstrumentUpdate> Instruments
);