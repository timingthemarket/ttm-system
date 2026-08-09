using TTM.Shared.Constants;
using TTM.Shared.Models;

namespace portfolio.Domain.Constants;

/// <summary>
/// One indicator evaluated in one direction. Indicators that are meaningful both when high
/// and when low (Volatility, RsiMomentum) appear twice and are scored as two independent sets.
/// </summary>
/// <param name="Indicator">The indicator to fetch from masterdata.</param>
/// <param name="Direction">Which end of the distribution is considered "good".</param>
/// <param name="LookBackDays">The time span of the indicator data, in days.</param>
/// <param name="Aggregate">
/// How masterdata should treat the look back period. Computed indicators
/// (see <see cref="TTM.Shared.Extensions.IndicatorExtensions.IsComputedIndicator"/>) calculate the
/// span themselves and take <see cref="Aggregator.Value"/>; non-computed indicators are read
/// straight from the indicators table and ignore the look back period entirely unless the
/// aggregator is Average or Sum.
/// </param>
public sealed record IndicatorStrengthSet(
    Indicators Indicator,
    Direction Direction,
    int LookBackDays,
    Aggregator Aggregate)
{
    /// <summary>Stable identity of the set, used to key the per-set observation series.</summary>
    public string Key => $"{Indicator}|{Direction}";

    public LookBackPeriod ToLookBackPeriod() => new() { Period = LookBackDays, Aggregate = Aggregate };
}

/// <summary>
/// The indicator sets scored by the indicator strength backtest. This is the single place to
/// add, remove or re-tune a set - everything downstream is driven off this list.
/// </summary>
public static class IndicatorStrengthSets
{
    public static IReadOnlyList<IndicatorStrengthSet> Sets { get; } = new List<IndicatorStrengthSet>
    {
        new(Indicators.Dividend, Direction.High, 365, Aggregator.Average),
        new(Indicators.Pe, Direction.High, 365, Aggregator.Value),
        new(Indicators.Volatility, Direction.High, 60, Aggregator.Value),
        new(Indicators.Volatility, Direction.Low, 60, Aggregator.Value),
        new(Indicators.Return, Direction.High, 180, Aggregator.Value),
        new(Indicators.RsiMomentum, Direction.High, 90, Aggregator.Value),
        new(Indicators.RsiMomentum, Direction.Low, 90, Aggregator.Value),
        new(Indicators.Roc, Direction.High, 365, Aggregator.Average),
        new(Indicators.Roic, Direction.High, 365, Aggregator.Average),
        new(Indicators.FScore, Direction.High, 365, Aggregator.Average)
    };
}
