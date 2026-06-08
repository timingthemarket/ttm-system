using System.Runtime.Serialization;

namespace TTM.Shared.Models.SecuritiesMasterdata.Dto;

[DataContract]
public class SecurityDto
{
    [DataMember(Order = 1, IsRequired = true)]
    public long SecurityId { get; set; }

    [DataMember(Order = 2, IsRequired = true)]
    public string Ticker { get; set; }

    [DataMember(Order = 3, IsRequired = true)]
    public string Name { get; set; }

    [DataMember(Order = 4, IsRequired = true)]
    public string Isin { get; set; }

    [DataMember(Order = 5, IsRequired = true)]
    public long MarketId { get; set; }

    [DataMember(Order = 6, IsRequired = true)]
    public string Market { get; set; }

    [DataMember(Order = 7, IsRequired = true)]
    public string YahooTicker { get; set; }

    [DataMember(Order = 8, IsRequired = true)]
    public long CurrencyId { get; set; }

    [DataMember(Order = 9, IsRequired = true)]
    public string CurrencyCode { get; set; }

    [DataMember(Order = 10)] public string? Industry { get; set; }

    [DataMember(Order = 11, IsRequired = true)]
    public string Sector { get; set; }

    [DataMember(Order = 12, IsRequired = true)]
    public string Country { get; set; }

    [DataMember(Order = 13)] public string? Description { get; set; }

    [DataMember(Order = 14, IsRequired = true)]
    public double LatestRawPrice { get; set; }

    [DataMember(Order = 15, IsRequired = true)]
    public DateTime Updated { get; set; }
}