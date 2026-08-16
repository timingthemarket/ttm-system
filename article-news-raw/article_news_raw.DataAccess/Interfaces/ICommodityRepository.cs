using article_news_raw.DataAccess.Models;

namespace article_news_raw.DataAccess.Interfaces;

public interface ICommodityRepository
{
    /// <summary>
    /// Inserts the given data points, overwriting the value of any that already exist.
    /// Every fetch re-reads the full history, so this has to be idempotent.
    /// </summary>
    /// <returns>The number of rows inserted or updated.</returns>
    Task<int> UpsertCommodities(List<Commodity> commodities, CancellationToken token = default);
}
