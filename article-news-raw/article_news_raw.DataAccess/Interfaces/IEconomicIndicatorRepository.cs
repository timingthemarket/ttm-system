using article_news_raw.DataAccess.Models;

namespace article_news_raw.DataAccess.Interfaces;

public interface IEconomicIndicatorRepository
{
    /// <summary>
    /// Inserts the given data points, overwriting the value of any that already exist.
    /// Every fetch re-reads the full history, so this has to be idempotent.
    /// </summary>
    /// <returns>The number of rows inserted or updated.</returns>
    Task<int> UpsertEconomicIndicators(List<EconomicIndicator> economicIndicators, CancellationToken token = default);

    /// <summary>
    /// Returns the stored data points for one indicator between <paramref name="dateFrom"/> and
    /// <paramref name="dateTo"/> (both inclusive), ordered by date.
    /// </summary>
    Task<List<EconomicIndicator>> GetEconomicIndicators(string indicatorType, DateOnly dateFrom, DateOnly dateTo, CancellationToken token = default);
}
