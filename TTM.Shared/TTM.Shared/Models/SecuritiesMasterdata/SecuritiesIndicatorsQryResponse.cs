using System.Runtime.Serialization;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace TTM.Shared.Models.SecuritiesMasterdata;

[DataContract]
public class SecuritiesIndicatorsQryResponse
{
    [DataMember(Order = 1, IsRequired = true)]
    public DateOnly Date { get; set; }

    [DataMember(Order = 2, IsRequired = true)]
    public required List<SecurityIndicatorDto> Variables { get; set; }
}