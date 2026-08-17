using article_news_raw.DataAccess.Models.Api;

namespace article_news_raw.DataAccess.Interfaces;

public interface IAlphaVantageIndexDataService
{
    Task<AlphaVantageIndex> GetSp500History(CancellationToken token = default);
    Task<AlphaVantageIndex> GetVixHistory(CancellationToken token = default);
}
