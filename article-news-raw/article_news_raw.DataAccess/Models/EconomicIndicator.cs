namespace article_news_raw.DataAccess.Models;

public class EconomicIndicator
{
    public DateOnly Date { get; set; }

    /// <summary>
    /// One of <see cref="TTM.Shared.Constants.EconomicIndicatorTypes"/>.
    /// </summary>
    public string IndicatorType { get; set; } = null!;

    public double Value { get; set; }
}
