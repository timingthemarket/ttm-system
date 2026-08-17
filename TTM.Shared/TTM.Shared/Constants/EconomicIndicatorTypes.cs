namespace TTM.Shared.Constants;

/// <summary>
/// The economic indicators article-news-raw stores history for. Values are what the
/// <c>indicator_type</c> column of <c>economic_indicator</c> holds, and double as the
/// <c>function</c> parameter of the AlphaVantage economic indicator endpoints.
/// </summary>
public static class EconomicIndicatorTypes
{
    /// <summary>
    /// US consumer price inflation. Alphavantage only publishes this annually.
    /// </summary>
    public const string Inflation = "INFLATION";

    /// <summary>
    /// The effective federal funds rate, fetched monthly.
    /// </summary>
    public const string FederalFundsRate = "FEDERAL_FUNDS_RATE";

    /// <summary>
    /// Every indicator that can be asked for, for callers that need to validate or enumerate them.
    /// </summary>
    public static readonly IReadOnlyList<string> All = [Inflation, FederalFundsRate];
}
