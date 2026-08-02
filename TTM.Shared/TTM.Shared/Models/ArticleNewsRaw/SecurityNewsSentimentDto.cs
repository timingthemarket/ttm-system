using System.Runtime.Serialization;

namespace TTM.Shared.Models.ArticleNewsRaw;

[DataContract]
public class SecurityNewsSentimentDto
{
    [DataMember(Order = 1)]
    public string Ticker { get; set; }
    [DataMember(Order = 2)]
    public int NrOccurances { get; set; }
    [DataMember(Order = 3)]
    public double AverageSentiment { get; set; }
}