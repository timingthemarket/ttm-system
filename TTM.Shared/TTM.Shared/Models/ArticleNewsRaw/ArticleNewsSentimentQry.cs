using System.Runtime.Serialization;

namespace TTM.Shared.Models.ArticleNewsRaw;

[DataContract]
public class ArticleNewsSentimentQry
{
    [DataMember(Order = 1, IsRequired = true)]
    public List<string> Tickers { get; set; }

    [DataMember(Order = 2)]
    public DateTime? From { get; set; }

    [DataMember(Order = 3)]
    public DateTime? To { get; set; }
}
