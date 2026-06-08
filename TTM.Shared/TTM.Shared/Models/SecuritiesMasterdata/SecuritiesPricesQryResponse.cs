using System.Runtime.Serialization;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace TTM.Shared.Models.SecuritiesMasterdata;

[DataContract]
public class SecuritiesPricesQryResponse
{
    [DataMember(Order = 1, IsRequired = true)]
    public List<SecurityPriceDto> SecurityPrices { get; set; }
}