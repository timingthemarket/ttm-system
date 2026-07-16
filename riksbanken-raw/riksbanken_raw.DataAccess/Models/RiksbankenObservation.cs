using System.Text.Json.Serialization;

namespace riksbanken_raw.DataAccess.Models;

public class RiksbankenObservation
{
    [JsonPropertyName("date")] public string Date { get; set; }

    [JsonPropertyName("value")] public double Value { get; set; }
}