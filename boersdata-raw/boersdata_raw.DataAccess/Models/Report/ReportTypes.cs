using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace boersdata_raw.DataAccess.Models.Report;

public record ReportTypes
{
    [BsonIgnoreIfDefault]
    public ObjectId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Translations Translations { get; set; } = new Translations();
    public string ReportProperty { get; set; } = string.Empty;
};