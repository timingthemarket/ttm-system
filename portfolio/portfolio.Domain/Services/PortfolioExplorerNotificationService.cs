using Microsoft.Extensions.Logging;
using portfolio.DataAccess.Interfaces;
using portfolio.DataAccess.Models.Db;
using portfolio.DataAccess.Models.Services;
using portfolio.DataAccess.Services;
using portfolio.Domain.Interfaces;

namespace portfolio.Domain.Services;

public class PortfolioExplorerNotificationService(ILogger<PortfolioExplorerNotificationService> logger, 
    ISimulationRepository simulationRepository, IPortfolioRepository portfolioRepository, 
    IMasterdataService masterdataService) : IPortfolioExplorerNotificationService
{
    public async Task ProcessPortfolioExplorerNotification()
    {
        logger.LogInformation("Processing latest PortfolioExplorer data...");
        
        var latestBestSimulation = await simulationRepository.GetLatestBestSimulation();
        var bestPortfolioTask = portfolioRepository.GetPortfolioFromSimulationId(latestBestSimulation.Id);
        
        var latestSimulationAfter = await simulationRepository.GetBestSimulationByDate(latestBestSimulation.SecuritiesDate);
        var portfolioAfter = latestSimulationAfter == null ? null : await portfolioRepository.GetPortfolioFromSimulationId(latestSimulationAfter.Id);
        
        var bestPortfolio = await bestPortfolioTask;
        if (bestPortfolio == null)
            throw new NullReferenceException("basePortfolio is null");
        
        var securityIds = bestPortfolio.PortfolioValues.Select(pv => pv.SecurityId).ToList();
        var securities = await masterdataService.GetSecurites(null, securityIds);
        
        var discordService = new DiscordService();
        await discordService.SendPortfolioUpdateNotification(new()
        {
            SessionDate = latestBestSimulation.SecuritiesDate,
            SessionPortfolio = new ()
            {
                Id = latestBestSimulation.Id,
                RowSimilarity = bestPortfolio.RowSimilarity,
                SecuritiesDate = bestPortfolio.SecuritiesDate,
                Money = latestBestSimulation.InitMoney,
                PortfolioPercentageChange = latestBestSimulation.PercentageChange!.Value,
                Securities = bestPortfolio.PortfolioValues.Select(p =>
                {
                    var security = securities.Securities.First(s => s.SecurityId == p.SecurityId);
                    return new SessionSecurity
                    {
                        Amount = p.Amount,
                        Rank = p.Rank,
                        SecurityId = p.SecurityId,
                        Ticker = security.Ticker,
                        Sector = security.Sector
                    };
                }).ToList()
            },
            NewIndicators = bestPortfolio.PortfolioIndicators.Select(i => new SessionIndicator
            {
                Direction = i.Direction,
                Indicator = i.Indicator,
                LookBackPeriod = i.LookbackPeriod,
                LookBackAggregator = i.LookbackAggregator
            }).ToList(),
            OldIndicators = portfolioAfter == null ? new () : portfolioAfter.PortfolioIndicators
                .Select(i => new SessionIndicator
            {
                Direction = i.Direction,
                Indicator = i.Indicator,
                LookBackPeriod = i.LookbackPeriod,
                LookBackAggregator = i.LookbackAggregator
            }).ToList()
        });
        
        logger.LogInformation("Done processing latest PortfolioExplorer data!");
        
        // TODO: Add masstransit saga, where this function executes first and then after a computation of the new portoflio with the choosen indicators
    }
}