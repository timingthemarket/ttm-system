using System.Runtime.Serialization;

namespace TTM.Shared.Models.BoersDataRaw;

[DataContract]
public class HistoricalReportsQry
{
    [DataMember(Order = 1)]
    public List<string> Tickers { get; set; }
}