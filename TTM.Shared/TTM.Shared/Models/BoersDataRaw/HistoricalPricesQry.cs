using System.Runtime.Serialization;

namespace TTM.Shared.Models.BoersDataRaw;

[DataContract]
public class HistoricalPricesQry
{
    [DataMember(Order = 1)]
    public List<string> Tickers { get; set; }
}
