namespace article_news_raw.DataAccess.Models;

public class ArticleTickerSentiment
{
    public int Id { get; set; }
    public int ArticleUrlId { get; set; }
    public string Ticker { get; set; } = null!;
    public double SentimentScore { get; set; }
    public double RelevanceScore { get; set; }

    public ArticleUrl ArticleUrl { get; set; } = null!;
}