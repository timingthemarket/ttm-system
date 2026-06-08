using System.Runtime.Serialization;
using TTM.Shared.Constants;

namespace TTM.Shared.Models.SecuritiesMasterdata.Dto;

[DataContract]
public class SecuritiesIndicatorQryMetadataDto
{
    [DataMember(Order = 1, IsRequired = true)]
    public Indicators IndicatorId { get; set; }

    [DataMember(Order = 2)]
    public LookBackPeriod? LookBackPeriod { get; set; }
}