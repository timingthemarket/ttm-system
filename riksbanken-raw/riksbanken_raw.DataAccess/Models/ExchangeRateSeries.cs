using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace riksbanken_raw.DataAccess.Models;

public class ExchangeRateSeries
{
    [BsonIgnoreIfDefault] public ObjectId Id { get; set; }
    public string SeriesId { get; set; }
    public string Source { get; set; }
    public string ShortDescription { get; set; }
    public int GroupId { get; set; }
    public DateTime? LastFetched { get; set; }
}