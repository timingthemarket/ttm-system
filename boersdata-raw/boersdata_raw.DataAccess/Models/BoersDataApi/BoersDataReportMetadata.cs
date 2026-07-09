using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

public record BoersDataReportMetadata(
    [property: JsonPropertyName("reportPropery")] string ReportPropery,
    [property: JsonPropertyName("nameSv")] string NameSv,
    [property: JsonPropertyName("nameEn")] string NameEn,
    [property: JsonPropertyName("format")] string? Format
);

public record BoersDataReportMetadatas(
    [property: JsonPropertyName("reportMetadatas")] IReadOnlyList<BoersDataReportMetadata> ReportMetadatas
);