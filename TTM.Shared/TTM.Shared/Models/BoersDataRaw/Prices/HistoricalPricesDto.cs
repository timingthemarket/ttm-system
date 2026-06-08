using System.Runtime.Serialization;

namespace TTM.Shared.Models.BoersDataRaw.Prices;

[DataContract]
public class HistoricalPricesDto
{
    [DataMember(Order = 1)]
    public string Ticker { get; set; }

    [DataMember(Order = 2)]
    public List<SecurityPriceDto> HistoricalPrices { get; set; }
}