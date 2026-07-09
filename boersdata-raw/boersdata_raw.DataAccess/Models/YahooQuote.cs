using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace boersdata_raw.DataAccess.Models;

public class YahooQuote
{
    [BsonIgnoreIfDefault] public ObjectId Id { get; set; }
    public string YahooTicker { get; set; }
    public string? Currency { get; set; }
    public string? FinancialCurrency { get; set; }
    public long BoersDataInstrumentId { get; set; }
}