using System.Runtime.Serialization;

namespace TTM.Shared.Models.BoersDataRaw.Securities;

[DataContract]
public class SecurityDto
{
    [DataMember(Order = 1, IsRequired = true)]
    public string Ticker { get; init; } = string.Empty;

    [DataMember(Order = 2, IsRequired = true)]
    public string Name { get; init; } = string.Empty;

    [DataMember(Order = 3)]
    public string Isin { get; set; } = string.Empty;

    [DataMember(Order = 4, IsRequired = true)]
    public string Type { get; set; }

    [DataMember(Order = 5, IsRequired = true)]
    public MarketDto Market { get; set; }

    [DataMember(Order = 6, IsRequired = true)]
    public CountryDto Country { get; set; }

    [DataMember(Order = 7, IsRequired = true)]
    public SectorDto Sector { get; set; }

    [DataMember(Order = 8)]
    public IndustryDto? Industry { get; set; }

    [DataMember(Order = 9, IsRequired = true)]
    public string Currency { get; set; }

    [DataMember(Order = 10, IsRequired = true)]
    public string YahooTicker { get; set; }
}