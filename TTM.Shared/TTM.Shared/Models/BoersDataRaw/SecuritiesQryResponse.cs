using System.Runtime.Serialization;
using TTM.Shared.Models.BoersDataRaw.Securities;

namespace TTM.Shared.Models.BoersDataRaw;

[DataContract]
public class SecuritiesQryResponse
{
    [DataMember(Order = 1)]
    public List<SecurityDto> Securities { get; set; }
}