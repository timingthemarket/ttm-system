using System.Runtime.Serialization;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace TTM.Shared.Models.SecuritiesMasterdata;

[DataContract]
public class SecuritiesQryResponse
{
    [DataMember(Order = 1, IsRequired = true)]
    public List<SecurityDto> Securities { get; set; }
}