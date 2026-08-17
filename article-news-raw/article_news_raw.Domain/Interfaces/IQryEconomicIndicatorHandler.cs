using TTM.Shared.Models.ArticleNewsRaw;

namespace article_news_raw.Domain.Interfaces;

public interface IQryEconomicIndicatorHandler
{
    Task<List<EconomicIndicatorDto>> HandleGetEconomicIndicators(string indicatorType, DateOnly dateFrom, DateOnly dateTo, CancellationToken token = default);
}
