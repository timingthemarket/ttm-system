using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace riksbanken_raw.DataAccess.Models;

[BsonIgnoreExtraElements]
public class CurrencyRate
{
    [BsonIgnoreIfDefault] public ObjectId Id { get; set; }
    public string Date { get; set; }
    public double Rate { get; set; }
    public string FromCode { get; set; }
    public string ToCode { get; set; }
}