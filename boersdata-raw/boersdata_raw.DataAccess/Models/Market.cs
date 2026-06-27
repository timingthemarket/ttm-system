using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace boersdata_raw.DataAccess.Models;

public sealed record Market
{
    [BsonIgnoreIfDefault] public ObjectId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long MarketId { get; set; }
}