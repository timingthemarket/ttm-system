using MathNet.Numerics.Statistics;

namespace portfolio.Domain.Utils;

/// <summary>
/// The statistics behind the indicator strength metric.
/// </summary>
/// <remarks>
/// Deliberately does not use <see cref="Functions.StandardDeviation"/> - that helper is missing the
/// squaring term and returns NaN/0. MathNet is already a dependency of portfolio.Domain.
/// </remarks>
public static class StrengthStatistics
{
    /// <summary>Number of trailing monthly observations feeding the rolling Sharpe and mean IC.</summary>
    public const int RollingWindowMonths = 6;

    /// <summary>Below this many observations a rolling value is considered too noisy to publish.</summary>
    public const int MinObservations = 6;

    /// <summary>Number of monthly periods in a year, used to annualise the Sharpe ratio.</summary>
    public const int PeriodsPerYear = 12;

    public const double SharpeWeight = 0.5;
    public const double IcWeight = 0.3;

    /// <summary>
    /// A constant return series does not produce an exactly zero standard deviation - floating
    /// point noise around 1e-18 survives, which would turn into an astronomical Sharpe ratio.
    /// Anything below this is treated as no dispersion at all.
    /// </summary>
    private const double MinStandardDeviation = 1e-9;

    /// <summary>
    /// Annualised Sharpe ratio of a series of monthly returns, assuming a risk free rate of zero.
    /// Returns null when there is too little data or the returns have no dispersion.
    /// </summary>
    public static double? Sharpe(IReadOnlyList<double> monthlyReturns)
    {
        if (monthlyReturns.Count < MinObservations) return null;

        var mean = monthlyReturns.Mean();
        var stdDev = monthlyReturns.StandardDeviation();

        if (double.IsNaN(stdDev) || stdDev < MinStandardDeviation) return null;

        var sharpe = mean / stdDev * Math.Sqrt(RollingWindowMonths); // Math.Sqrt(PeriodsPerYear);
        return double.IsFinite(sharpe) ? sharpe : null;
    }

    /// <summary>
    /// Information Coefficient for a single period: the Spearman rank correlation between the
    /// factor scores observed at a date and the returns realised over the following month.
    /// Rank correlation rather than Pearson because indicator values are on wildly different
    /// scales and often heavy tailed.
    /// Returns null when there is too little data or either side is completely tied.
    /// </summary>
    public static double? InformationCoefficient(IReadOnlyList<double> factorScores,
        IReadOnlyList<double> forwardReturns)
    {
        if (factorScores.Count != forwardReturns.Count) return null;
        if (factorScores.Count < MinObservations) return null;
        if (factorScores.Distinct().Count() < 2 || forwardReturns.Distinct().Count() < 2) return null;

        var ic = Correlation.Spearman(factorScores, forwardReturns);
        return double.IsFinite(ic) ? ic : null;
    }

    /// <summary>
    /// The trailing slice of <paramref name="observations"/> ending immediately before
    /// <paramref name="exclusiveEndIndex"/>. Callers pass the index of the date being scored, so the
    /// observation starting at that date - which is only knowable a month later - is never included.
    /// </summary>
    public static List<double> RollingWindow(IReadOnlyList<double?> observations, int exclusiveEndIndex,
        int windowSize = RollingWindowMonths)
    {
        var start = Math.Max(0, exclusiveEndIndex - windowSize);
        var window = new List<double>(windowSize);

        for (var i = start; i < exclusiveEndIndex && i < observations.Count; i++)
            if (observations[i].HasValue)
                window.Add(observations[i]!.Value);

        return window;
    }

    /// <summary>
    /// Min-max rescale to [0,1] across the values of a single rebalance date. When every value is
    /// identical there is no spread to rank on, so all of them score the neutral midpoint rather
    /// than dividing by zero.
    /// </summary>
    public static double Normalize(double value, double min, double max) =>
        max > min ? Functions.Normalize01(value, min, max) : 0.5;

    /// <summary>
    /// Strength = 0.5 x normalized Sharpe + 0.3 x normalized IC. Range [0, 0.8].
    /// </summary>
    public static double Strength(double normalizedSharpe, double normalizedIc) =>
        SharpeWeight * normalizedSharpe + IcWeight * normalizedIc;
}
