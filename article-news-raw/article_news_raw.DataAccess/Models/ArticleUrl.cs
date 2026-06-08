
namespace article_news_raw.DataAccess.Models;

public class ArticleUrl
{
    
    public int Id { get; set; }
    public string Url { get; set; } = null!;

    public DateTime DateUrlFetched { get; set; }
    
    public DateTime? DateArticlePublished { get; set; }
    public bool IsContentRead { get; set; }
    public bool IsParsed { get; set; }
    public bool IsBad { get; set; }

    public List<ArticleTickerSentiment> TickerSentiments { get; set; } = new();
}