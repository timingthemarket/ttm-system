using article_news_raw.DataAccess.Models.Api;

namespace article_news_raw.DataAccess.Interfaces;

public interface IAlphaVantageEconomicIndicatorsService
{
    Task<AlphaVantageEconomicIndicator> GetInflationHistory(CancellationToken token = default);
    Task<AlphaVantageEconomicIndicator> GetFederalFundsRateHistory(CancellationToken token = default);
}
