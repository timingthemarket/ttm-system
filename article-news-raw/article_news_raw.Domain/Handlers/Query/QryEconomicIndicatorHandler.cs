using article_news_raw.DataAccess.Interfaces;
using article_news_raw.DataAccess.Models;
using article_news_raw.Domain.Interfaces;
using TTM.Shared.Models.ArticleNewsRaw;

namespace article_news_raw.Domain.Handlers.Query;

public class QryEconomicIndicatorHandler(IEconomicIndicatorRepository economicIndicatorRepository) : IQryEconomicIndicatorHandler
{
    public async Task<List<EconomicIndicatorDto>> HandleGetEconomicIndicators(string indicatorType, DateOnly dateFrom,
        DateOnly dateTo, CancellationToken token = default)
    {
        var economicIndicators =
            await economicIndicatorRepository.GetEconomicIndicators(indicatorType, dateFrom, dateTo, token);
        return MapToEconomicIndicatorDtos(economicIndicators);
    }

    private static List<EconomicIndicatorDto> MapToEconomicIndicatorDtos(List<EconomicIndicator> economicIndicators) =>
        economicIndicators
            .Select(e => new EconomicIndicatorDto
            {
                Date = e.Date,
                IndicatorType = e.IndicatorType,
                Value = e.Value
            })
            .ToList();
}
