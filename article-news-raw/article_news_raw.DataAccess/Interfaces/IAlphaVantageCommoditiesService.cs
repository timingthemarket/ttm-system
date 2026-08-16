using article_news_raw.DataAccess.Models.Api;

namespace article_news_raw.DataAccess.Interfaces;

public interface IAlphaVantageCommoditiesService
{
    Task<AlphaVantageCommodity> GetGoldHistory(CancellationToken token = default);
    Task<AlphaVantageCommodity> GetSilverHistory(CancellationToken token = default);
    Task<AlphaVantageCommodity> GetBrentCrudeOilHistory(CancellationToken token = default);
}
