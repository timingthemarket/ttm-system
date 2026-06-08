using System.Runtime.Serialization;

namespace TTM.Shared.Models.BoersDataRaw.Prices;

[DataContract]
public class SecurityPriceDto
{
    [DataMember(Order = 1)]
    public string Ticker { get; set; }

    [DataMember(Order = 2)] public double? Open { get; set; }

    [DataMember(Order = 3)] public double? Close { get; set; }

    [DataMember(Order = 4)] public double? High { get; set; }

    [DataMember(Order = 5)] public double? Low { get; set; }

    [DataMember(Order = 6)] public long? Volume { get; set; }

    [DataMember(Order = 7)]
    public DateOnly Date { get; set; }
}
