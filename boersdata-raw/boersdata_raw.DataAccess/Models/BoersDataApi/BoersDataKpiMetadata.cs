using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

public record InstrumentsKpiHistory(
    [property: JsonPropertyName("kpiId")] long KpiId,
    [property: JsonPropertyName("reportTime")] string ReportTime,
    [property: JsonPropertyName("priceValue")] string PriceValue,
    [property: JsonPropertyName("kpisList")] IReadOnlyList<InstrumentKpi> KpisList
);


public record InstrumentKpi(
    [property: JsonPropertyName("instrument")] long InsId,
    [property: JsonPropertyName("values")] IReadOnlyList<KpiValue> KpiValues
);

public record KpiValue(
    [property: JsonPropertyName("y")] int Year,
    [property: JsonPropertyName("p")] int Quarter,
    [property: JsonPropertyName("v")] double? Value
);