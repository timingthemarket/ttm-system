using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace boersdata_raw.DataAccess.Models;

public enum SecurityType
{
    Stocks = 0,
    Pref = 1,
    Index = 2, // No sector or branch -id
    Currency = 3,
    Commodity = 4,
    Spac = 5,
    Adr = 6,
    Unit = 7,
    Cryptocurrencies = 8
}

[BsonIgnoreExtraElements]
public record Security
{
    [BsonIgnoreIfDefault] public ObjectId Id { get; set; }

    public string Ticker { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Isin { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)] public SecurityType Type { get; set; }

    public long MarketId { get; set; }
    public long CountryId { get; set; }
    public long? SectorId { get; set; }
    public long? IndustryId { get; set; }
    public string YahooTicker { get; set; } = string.Empty;
    public long InsId { get; set; }
    public string UrlName { get; set; } = string.Empty;
    public string Currency { get; set; }
    public string ReportCurrency { get; set; }
}