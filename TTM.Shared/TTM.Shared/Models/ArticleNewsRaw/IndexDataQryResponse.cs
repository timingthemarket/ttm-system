using System.Runtime.Serialization;

namespace TTM.Shared.Models.ArticleNewsRaw;

[DataContract]
public class IndexDataQryResponse
{
    [DataMember(Order = 1, IsRequired = true)]
    public List<IndexDataDto> IndexData { get; set; }
}
