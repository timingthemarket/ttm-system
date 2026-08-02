using TTM.Shared.Models.ArticleNewsRaw;

namespace article_news_raw.Domain.Models.SectorSentiment;

public class SectorSentimentAggregateDto
{
    public string Sector { get; set; } = null!;
    public double WeightedAverageSentiment { get; set; }
    public double SimpleAverageSentiment { get; set; }
    public int TotalOccurrences { get; set; }
    public List<SecurityNewsSentimentDto> TopByAverageSentiment { get; set; } = [];
    public List<SecurityNewsSentimentDto> TopByOccurrences { get; set; } = [];
}
