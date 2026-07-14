using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace boersdata_raw.DataAccess.Models;

public record Country
{
    [BsonIgnoreIfDefault]
    public ObjectId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Translations Translations { get; set; } = new Translations();
    public long CountryId { get; set; }
};