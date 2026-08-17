using System.Runtime.Serialization;

namespace TTM.Shared.Models.ArticleNewsRaw;

[DataContract]
public class EconomicIndicatorQryResponse
{
    [DataMember(Order = 1, IsRequired = true)]
    public List<EconomicIndicatorDto> EconomicIndicators { get; set; }
}
