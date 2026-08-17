namespace article_news_raw.DataAccess.Models;

public class IndexData
{
    public DateOnly Date { get; set; }

    /// <summary>
    /// One of <see cref="IndexTypes"/>.
    /// </summary>
    public string IndexType { get; set; } = null!;

    public double Value { get; set; }
}
