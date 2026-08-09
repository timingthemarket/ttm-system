using System.Text.Json.Serialization;

namespace securities_masterdata.DataAccess.Services.Models;

public class NordnetStocklistResponse
{
    [JsonPropertyName("rows")]
    public int Rows { get; set; }

    [JsonPropertyName("total_hits")]
    public int TotalHits { get; set; }

    [JsonPropertyName("results")]
    public NordnetInstrument[] Results { get; set; } = [];
}

public class NordnetInstrument
{
    [JsonPropertyName("instrument_info")]
    public NordnetInstrumentInfo InstrumentInfo { get; set; } = new();
}

public class NordnetInstrumentInfo
{
    [JsonPropertyName("symbol")]
    public string Ticker { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
