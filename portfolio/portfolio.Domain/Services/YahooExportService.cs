using portfolio.DataAccess.Interfaces;
using portfolio.Domain.Constants;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Models;
using TTM.Shared.Constants;

namespace portfolio.Domain.Services;

public class YahooExportService(IPortfolioRepository portfolioRepository, IPortfolio portfolioCompute, IYahooCsvFileHandler yahooCsvFileHandler) : IYahooExportService
{
    public async Task<Stream> ExportYahooDataToFile(decimal money, Guid portfolioId)
    {
        var portoflioToBaseExport = await portfolioRepository.GetPortfolioById(portfolioId);
        if (portoflioToBaseExport is null)
            throw new NullReferenceException("simulation is null");
        
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var portfolio = await portfolioCompute.Compute(new()
        {
            Date = date,
            Money = money,
            RowSimilarity = InternalSettings.DefaultRowSimilarity,
            StrategyId = 1,
            MaxSecuritySpending = (double)money * 0.05,
            Indicators = portoflioToBaseExport.PortfolioIndicators.Select(i =>
            {
                var indicatorVar = new PortfolioInputIndicatorVariable
                {
                    Direction = i.Direction,
                    IndicatorId = i.Indicator,
                    LookBackPeriod = i.LookbackAggregator.HasValue && i.LookbackPeriod.HasValue
                        ? new()
                        {
                            Aggregate = i.LookbackAggregator.Value,
                            Period = i.LookbackPeriod.Value
                        }
                        : null,
                    Weight = i.Weight
                };

                // TODO: save this info in the DB
                if (i.Indicator == Indicators.Dividend)
                    indicatorVar.ImputationStrategy = new StrategyImputation
                        { Action = MissingDataAction.Value, ImputationValue = 0 };
                
                return indicatorVar;
            }).ToList(),
        }, savePortfolio: false);

        var file = await yahooCsvFileHandler.HandleMakeYahooCsvFile(portfolio);
        return file;
    }

    public async Task<Stream> ExportYahooDataToFileBySetId(decimal money, string setId)
    {
        var portfolioId = await portfolioRepository.GetPortfolioIdBySetId(setId);
        if (portfolioId is null)
            throw new NullReferenceException($"No portfolio found for set_id: {setId}");

        return await ExportYahooDataToFile(money, portfolioId.Value);
    }
}