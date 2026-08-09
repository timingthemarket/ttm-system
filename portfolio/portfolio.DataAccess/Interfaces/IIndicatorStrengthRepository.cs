using portfolio.DataAccess.Models.Db;
using TTM.Shared.Constants;

namespace portfolio.DataAccess.Interfaces;

public interface IIndicatorStrengthRepository
{
    /// <summary>
    /// Replaces every strength row on the given date with <paramref name="strengths"/>.
    /// Makes a re-run of the same rebalance date idempotent.
    /// </summary>
    Task SaveMany(DateOnly date, List<IndicatorStrength> strengths);

    Task<List<IndicatorStrength>> GetByDate(DateOnly date);

    Task<List<IndicatorStrength>> GetByIndicator(Indicators indicator, Direction direction,
        DateOnly? fromDate = null, DateOnly? toDate = null);

    Task<IndicatorStrength?> GetLatestForIndicator(Indicators indicator, Direction direction);

    /// <summary>
    /// The strength values from the most recent date that has any data.
    /// </summary>
    Task<List<IndicatorStrength>> GetLatestForAllIndicators();

    Task<DateOnly?> GetLatestDate();

    Task DeleteByDate(DateOnly date);
}
