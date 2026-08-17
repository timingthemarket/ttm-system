namespace TTM.Shared.Constants;

/// <summary>
/// The indexes article-news-raw stores history for. Values are what the
/// <c>index_type</c> column of <c>index_data</c> holds, and double as the
/// <c>symbol</c> parameter of the AlphaVantage <c>INDEX_DATA</c> function.
/// </summary>
public static class IndexTypes
{
    public const string Sp500 = "SPX";
    public const string Vix = "VIX";

    /// <summary>
    /// Every index that can be asked for, for callers that need to validate or enumerate them.
    /// </summary>
    public static readonly IReadOnlyList<string> All = [Sp500, Vix];
}
