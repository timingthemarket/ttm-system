using Google.OrTools.LinearSolver;
using Google.OrTools.Sat;
using Microsoft.Extensions.Logging;
using portfolio.DataAccess.Interfaces;
using portfolio.DataAccess.Models.Db;
using portfolio.Domain.Constants;
using portfolio.Domain.Extensions;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Models;
using portfolio.Domain.Portfolio.Components;
using TTM.Shared.Constants;
using TTM.Shared.Models.PortfolioSimulation;
using TTM.Shared.Models.SecuritiesMasterdata;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace portfolio.Domain.Portfolio.Factory.StrategyModules;

public class DiLegacyStrategy(
    ILogger<DiLegacyStrategy> logger,
    IMasterdataService masterdataService)
    : IStrategy
{
    public Strategy Strategy => Strategy.DiLegacy;

    public async Task<DataAccess.Models.Db.Portfolio> Compute(StrategyInput input)
    {
        if (!ValidateCountryWeights(input))
            throw new Exception("Country weights are invalid! (>1)");
        
        if (!ValidateSectorWeights(input))
            throw new Exception("Sector weights are invalid! (>1)");
        
        var id = Guid.NewGuid();
        
        var variableIds = input.StrategyVariables.Select(v => new SecuritiesIndicatorQryMetadataDto
        {
            IndicatorId = v.IndicatorId,
            LookBackPeriod = v.LookBackPeriod
        }).ToList();
        SecuritiesIndicatorsQryResponse finVariables = await masterdataService.GetIndicators(input.Date, variableIds);

        var securites = await masterdataService.GetSecurites(null, null);
        
        var indicatorData = MakeIndicatorData(finVariables.Variables);

        var transformedData = indicatorData.Transform(input, securites.Securities);

        var ranks = transformedData.Rank(input);
        if (!ranks.Any()) // Just return the non valid portolio
            return MapPortfolio(id, input, new List<PortfolioValueDto>());

        var rankData = new RankingValueFunctionTransform().ApplyFunction(ranks);

        var portfolioSummary = await SummarizePortfolio(input, rankData, securites.Securities);
        
        return MapPortfolio(id, input, portfolioSummary);
    }

    private bool ValidateCountryWeights(StrategyInput input)
    {
        var cwSum = input.CountryWeight.Select(c => c.Value).Sum();
        return cwSum <= 1;
    }
    
    private bool ValidateSectorWeights(StrategyInput input)
    {
        var cwSum = input.SectorWeight.Select(c => c.Value).Sum();
        return cwSum <= 1;
    }

    private List<IndicatorData> MakeIndicatorData(List<SecurityIndicatorDto> dtos) =>
        dtos.Select(d => new IndicatorData(d.SecurityId, d.Value, d.IndicatorId, d.RankFriendlyValue)).ToList();

    /// <summary>
    /// Returns only the stocks that got allocated
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ranks"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<List<PortfolioValueDto>> SummarizePortfolio(StrategyInput input, List<FunctionSecurityRank> ranks, List<SecurityDto> securities)
    {
        var latestPricesTask =
            masterdataService.GetLatestPrices(input.Date, null);

        var securityPrices = await latestPricesTask;
        
        // TODO: base the securities on if the security has been in a previous portfolio

        var securityRanksJoin = ranks.Join(securities, r => r.SecurityId, s => s.SecurityId,
            (rank, security) => new { Security = security, Rank = rank })
            .Join(securityPrices.SecurityPrices, r => r.Security.SecurityId, s => s.SecurityId,
                (securityRank, sPrice) => new { securityRank.Security, securityRank.Rank, Price = sPrice })
            .Select(joins => new InternalSecurityRank(joins.Security, joins.Price, joins.Rank, 0))
            .Where(j =>
            {
                var medianPrice = j.Price.MedianPrice();
                if (medianPrice < input.MaxSecuritySpending) return true; // IF the price of a single security is larger then allowed security price, then remove it
                
                return medianPrice > 0; // price has to be positive
            })
            .ToList();
        
        if (!securityRanksJoin.Any())
            throw new Exception($"There are no securites remaining with MaxSecuritySpending: {input.MaxSecuritySpending}");

        var allocations = RunAllocations(input, securityRanksJoin);
        
        return allocations.Where(a => a.Amount > 0).ToList();
    }

    private List<PortfolioValueDto> RunAllocations(StrategyInput input, List<InternalSecurityRank> internalSecurity)
    {
        //TODO: remove all securities in a sector if the amount of money is lower then the lowest price security in that sector
        
        var allocator = new DiLegacyLpAllocator(input, internalSecurity);

        var portfolioValues = allocator.AllocateWithSectorCountrySecuritiesConstraint(out var resultStatus);
        if (resultStatus == CpSolverStatus.Optimal)
        {
            logger.LogInformation("AllocateWithSectorCountrySecuritiesConstraint completed with status {St}", resultStatus);
            return portfolioValues;
        }
        
        portfolioValues = allocator.AllocateWithOnlySectorConstraint(out resultStatus);
        
        if (resultStatus == CpSolverStatus.Optimal)
        {
            logger.LogInformation("AllocateWithOnlySectorConstraint completed with status {St}", resultStatus);
            return portfolioValues;
        }

        portfolioValues = allocator.AllocateWithOnlySecurityConstraint(out resultStatus);
        logger.LogInformation("Running AllocateWithOnlySecurityConstraint completed with status {St}",
            resultStatus);
        if (resultStatus == CpSolverStatus.Optimal)
        {
            return portfolioValues;
        }
        
        return portfolioValues;
    }

    private DataAccess.Models.Db.Portfolio MapPortfolio(Guid id, StrategyInput input, List<PortfolioValueDto> portfolioValues) =>
        new()
        {
            Strategy = Strategy,
            Id = id,
            Hash = input.Hash,
            RowSimilarity = input.RowSimilarityLimit,
            SecuritiesDate = input.Date,
            CalculationDate = DateTime.UtcNow,
            PortfolioValues = portfolioValues
                .Select(pv => new PortfolioValue
            {
                SecurityId = pv.SecurityId,
                Weight = pv.Weight,
                Rank = pv.Rank,
                Amount = pv.Amount,
                Price = pv.Price,
                Id = Guid.NewGuid(),
                PortfolioId = id
            }).ToList(),
            PortfolioIndicators = input.StrategyVariables.Select(pi => new PortfolioIndicator
            {
                Direction = pi.Direction,
                PortfolioId = id,
                Indicator = pi.IndicatorId,
                LookBack = $"{pi.LookBackPeriod?.Aggregate}|{pi.LookBackPeriod?.Period}",
                LookbackPeriod = pi.LookBackPeriod?.Period,
                LookbackAggregator = pi.LookBackPeriod?.Aggregate,
                Weight = pi.Weight ?? 0 // Zero wight if does not exist
            }).ToList()
        };
}