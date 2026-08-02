using System.Runtime.Serialization;

namespace TTM.Shared.Models.ArticleNewsRaw;

[DataContract]
public class ArticleNewsSentimentQryResponse
{
    [DataMember(Order = 1, IsRequired = true)]
    public List<SecurityNewsSentimentDto> SecurityNewsSentiments { get; set; }
}
