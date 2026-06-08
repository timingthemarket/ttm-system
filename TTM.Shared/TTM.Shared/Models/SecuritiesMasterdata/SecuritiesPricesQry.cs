using System.Runtime.Serialization;

namespace TTM.Shared.Models.SecuritiesMasterdata;

[DataContract]
public class SecuritiesPricesQry
{
    [DataMember(Order = 1, IsRequired = true)]
    public DateOnly Date { get; set; }
    [DataMember(Order = 2, IsRequired = true)]
    public HashSet<long> SecurityIds { get; set; }
}