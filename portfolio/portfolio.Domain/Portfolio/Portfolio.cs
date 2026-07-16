using Microsoft.Extensions.Logging;
using portfolio.DataAccess.Interfaces;
using portfolio.Domain.Constants;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Models;
using portfolio.Domain.Utils;
using TTM.Shared.Models.PortfolioSimulation;

namespace portfolio.Domain.Portfolio;

public class Portfolio(ILogger<Portfolio> logger, IPortfolioStrategyFactory strategyFactory, IPortfolioRepository portfolioRepository) : IPortfolio
{
    public async Task<PortfolioDto> Compute(PortfolioInput input, string? inputHash = null, bool savePortfolio = true)
    {
        inputHash ??= Functions.GetObjectHash(input);
        
        string indicators = string.Join("|", input.Indicators.Select(v => v.IndicatorId.ToString()));

        if (InternalSettings.Verbosity == LogVerbosity.All)
            logger.LogInformation("Portfolio: Date: {Date} MaxMoneyPerSecurity: {MaxMoneyPerSecurity} Money: {Money} RowSimilarity: {RowSimilarity} StrategyId: {StrategyId} Indicators: {Indicators}", 
                input.Date, input.MaxSecuritySpending, input.Money, input.RowSimilarity, input.StrategyId, indicators);
            
        var strategy = strategyFactory.GetStrategy(input.StrategyId);

        var portfolioVariables = MapStrategyInputVariable(input.Indicators);

        var computedPortfolio = await strategy.Compute(new StrategyInput
        {
            Date = input.Date,
            Hash = inputHash,
            StrategyVariables = portfolioVariables,
            RowSimilarityLimit = input.RowSimilarity,
            CountryWeight = input.CountryWeight,
            SectorWeight = input.SectorWeight,
            Money = input.Money,
            MaxSecuritySpending = input.MaxSecuritySpending
        });

        if (savePortfolio)
            await portfolioRepository.SavePortfolio(computedPortfolio);
        
        if (!computedPortfolio.PortfolioValues.Any())
            throw new Exception($"There are no ranks returned from ranking. PortfolioId {computedPortfolio.Id}");
        
        return MapPortfolioDto(computedPortfolio);
    }

    private List<StrategyInputVariable> MapStrategyInputVariable(List<PortfolioInputIndicatorVariable> variables)
    {
        return variables.Select(v => new StrategyInputVariable
        {
            Weight = v.Weight,
            IndicatorId = v.IndicatorId,
            LookBackPeriod = v.LookBackPeriod,
            Direction = v.Direction,
            Imputation = v.ImputationStrategy
        }).ToList();
    }

    private PortfolioDto MapPortfolioDto(DataAccess.Models.Db.Portfolio portfolio) => new PortfolioDto
    {
        Strategy = portfolio.Strategy,
        Id = portfolio.Id,
        SecuritiesDate = portfolio.SecuritiesDate,
        CalculationDate = portfolio.CalculationDate,
        PortfolioValues = portfolio.PortfolioValues
            .Select(pv => new PortfolioValueDto
            {
                SecurityId = pv.SecurityId,
                Weight = pv.Weight,
                Rank = pv.Rank,
                Amount = pv.Amount,
                Price = pv.Price
            }).ToList()
    };
}