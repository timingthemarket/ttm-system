using System.Runtime.Serialization;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace TTM.Shared.Models.SecuritiesMasterdata;

[DataContract]
public class SecuritiesIndicatorsQry
{
    [DataMember(Order = 1, IsRequired = true)]
    public DateOnly Date { get; set; }

    [DataMember(Order = 2, IsRequired = true)]
    public List<SecuritiesIndicatorQryMetadataDto> Indicators { get; set; }
}