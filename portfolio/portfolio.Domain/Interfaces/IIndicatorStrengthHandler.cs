namespace portfolio.Domain.Interfaces;

public interface IIndicatorStrengthHandler
{
    /// <summary>
    /// Backtests every set in <see cref="Constants.IndicatorStrengthSets"/> over the monthly
    /// rebalance grid ending at <paramref name="today"/> and persists a strength value per set
    /// per date.
    /// </summary>
    /// <param name="today">The last rebalance month to score.</param>
    /// <param name="backfillYears">How far back the grid reaches.</param>
    Task ProcessIndicatorStrength(DateOnly today, int backfillYears = 12,
        CancellationToken cancellationToken = default);
}
