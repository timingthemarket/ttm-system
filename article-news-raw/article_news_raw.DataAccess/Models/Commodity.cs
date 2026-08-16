namespace article_news_raw.DataAccess.Models;

public class Commodity
{
    public DateOnly Date { get; set; }

    /// <summary>
    /// One of <see cref="CommodityTypes"/>.
    /// </summary>
    public string CommodityType { get; set; } = null!;

    public double Value { get; set; }
}
