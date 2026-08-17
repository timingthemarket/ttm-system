namespace article_news_raw.DataAccess.Models;

/// <summary>
/// Values stored in the <c>index_type</c> column of <c>index_data</c>. They double as the
/// <c>symbol</c> parameter of the AlphaVantage <c>INDEX_DATA</c> function.
/// </summary>
public static class IndexTypes
{
    public const string Sp500 = "SPX";
    public const string Vix = "VIX";
}
