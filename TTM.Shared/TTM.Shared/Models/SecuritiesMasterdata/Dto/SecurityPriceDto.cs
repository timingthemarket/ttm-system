using System.Runtime.Serialization;

namespace TTM.Shared.Models.SecuritiesMasterdata.Dto;

[DataContract]
public class SecurityPriceDto
{
    [DataMember(Order = 1, IsRequired = true)]
    public long SecurityId { get; set; }
    [DataMember(Order = 2, IsRequired = true)]
    public DateOnly Date { get; set; }
    [DataMember(Order = 3, IsRequired = true)]
    public double Open { get; set; }
    [DataMember(Order = 4, IsRequired = true)]
    public double Close { get; set; }
    [DataMember(Order = 5, IsRequired = true)]
    public double High { get; set; }
    [DataMember(Order = 6, IsRequired = true)]
    public double Low { get; set; }
    [DataMember(Order = 7, IsRequired = true)]
    public long Volume { get; set; }
}