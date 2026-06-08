using System.Runtime.Serialization;
using TTM.Shared.Constants;

namespace TTM.Shared.Models.SecuritiesMasterdata.Dto;

[DataContract]
public class SecurityIndicatorDto
{
    [DataMember(Order = 1, IsRequired = true)]
    public Indicators IndicatorId { get; set; }

    [DataMember(Order = 2, IsRequired = true)]
    public long SecurityId { get; set; }

    [DataMember(Order = 3, IsRequired = true)]
    public DateOnly Date { get; set; }

    [DataMember(Order = 4, IsRequired = true)]
    public decimal Value { get; set; }

    [DataMember(Order = 5)]
    public decimal? RankFriendlyValue { get; set; } = null;
}