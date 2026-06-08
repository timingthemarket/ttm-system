using System.Runtime.Serialization;

namespace TTM.Shared.Models.SecuritiesMasterdata;

[DataContract]
public class SecuritiesQry
{
    [DataMember(Order = 1)]
    public List<string>? Tickers { get; set; }

    [DataMember(Order = 2)]
    public List<long>? SecurityIds { get; set; }

    [DataMember(Order = 3)]
    public bool ConvertPricesToOriginal { get; set; }
}