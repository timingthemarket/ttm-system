using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.Extensions.Logging;
using portfolio.DataAccess.Interfaces;
using portfolio.DataAccess.Models.Db;
using portfolio.Domain.Constants;
using portfolio.Domain.Extensions;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Models;
using portfolio.Domain.Serialization;
using portfolio.Domain.Utils;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;
using TTM.Shared.Functions;
using TTM.Shared.Models;
using TTM.Shared.Models.PortfolioSimulation;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;
using SimulationPeriod = portfolio.DataAccess.Models.Db.SimulationPeriod;

namespace portfolio.Domain.Services;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndicatorSearchSpace
{
    Start,
    End,
    Random
}

public class PortfolioExplorerService(
    ILogger<PortfolioExplorerService> logger,
    IPortfolioRepository portfolioRepository,
    IPortfolio portfolioService,
    IMasterdataService masterdataService,
    ISimulationRepository simulationResultRepository,
    IPublishEndpoint publishEndpoint) : IPortfolioExplorerHandler
{
    public async Task<bool> HandlePortfolioDiscover(int sessionId, DateOnly sessionDate, List<PortfolioInputIndicatorVariable> indicators, 
        HashSet<string> portfolioHashes, int initMoney, CancellationToken cancellationToken = default)
    {
        // Only log nessecary stuff
        InternalSettings.Verbosity = LogVerbosity.Warning;

        var nrTntervalDays = TimeIntervals.GetNrDaysForInterval(TimeIntervals.Monthly);
        
        var date = sessionDate.AddDays(-nrTntervalDays);
        
        //100_000, 250_000, 500_000, 1000_000
        // in SEK
        var maxSecuritySpending = GetMaxSecuritySpending(initMoney);

        // NOTE: this is a simulation but only for one period
        var input = new PortfolioInput
        {
            Date = date,
            Indicators = indicators,
            RowSimilarity = InternalSettings.DefaultRowSimilarity,
            StrategyId = 1,
            Money = initMoney,
            MaxSecuritySpending = maxSecuritySpending
        };
        
        var portfolioHash = Functions.GetObjectHash(input, HashSerializer.Default.PortfolioInput);
        
        // If a fetched portfolio hash already exists, skip the computation
        if (portfolioHashes.Contains(portfolioHash)) return false;
        
        // Check again if the portfolio with the same hash already exists in the database in case it was computed
        var savedPortfolio = await portfolioRepository.CheckPortfolioWithHash(portfolioHash);
        if (savedPortfolio)
        {
            // Portfolio already exists, no need to compute it again
            //logger.LogInformation("Portfolio with hash {Hash} already exists. Skipping computation", portfolioHash);
            await publishEndpoint.Increment(Metrics.PortfolioAlreadyComputed);
            return false;
        }
        
        logger.LogInformation("One period simulation STARTING for date {Date}", date);

        await RunClostetTimeToToday(sessionId, input, initMoney, portfolioHash, cancellationToken);

        logger.LogInformation("One period simulation DONE for date {Date}", date);
        return true;
    }
    
    private async Task RunClostetTimeToToday(int sessionId, PortfolioInput input, decimal initMoney, string inputHash,
        CancellationToken cancellationToken)
    {
        var portfolio = await portfolioService.Compute(input, inputHash);

        await publishEndpoint.Increment(Metrics.PortfolioComputed);

        var newPriceDate = DateOnly.FromDateTime(DateTime.Today);
        var investedMoney = portfolio.GetInvestedMoney();
        var liquidMoney = initMoney - investedMoney;
        var portfolioSecurities = portfolio.PortfolioValues.Select(v => v.SecurityId).ToHashSet();

        var newPrices = await masterdataService.GetLatestPrices(newPriceDate, portfolioSecurities, cancellationToken);

        var difference = CalculatePortfolioDifference((double)initMoney, (double)liquidMoney, newPrices.SecurityPrices,
            portfolio);

        logger.LogInformation("Portfolio resulted in {Increase}%", Math.Round(difference.Fraction * 100, 2));

        var simId = Guid.NewGuid();
        simulationResultRepository.SaveSimulation(new Simulation
        {
            Id = simId,
            Registered = DateTime.UtcNow,
            InitMoney = initMoney,
            Completed = DateTime.UtcNow,
            PercentageChange = difference.Fraction,
            SessionId = sessionId
        });

        var period = new SimulationPeriod
        {
            SimulationId = simId,
            InvestedMoney = investedMoney,
            LiquidMoney = liquidMoney,
            PortfolioId = portfolio.Id
        };

        simulationResultRepository.SaveSimulationPeriod(period);
    }

    private double GetMaxSecuritySpending(int totalMoney) { return totalMoney * 0.05; }

    private static (double Fraction, double PeriodPortofolioPrice) CalculatePortfolioDifference(
        double initPortfolioPrice, double liquidMoney,
        List<SecurityPriceDto> newPortfolioPrices, PortfolioDto prevPortfolio)
    {
        // Join the previous portfolios outcome with the new prices
        var periodPortfolioPrice = prevPortfolio.PortfolioValues.Join(newPortfolioPrices, pv => pv.SecurityId,
                pp => pp.SecurityId, (
                    portfolioValue, prices) => new { PortfolioValue = portfolioValue, Prices = prices })
            .Sum(join => join.Prices.MedianPrice() * join.PortfolioValue.Amount);

        var fraction = SharedFunctions.CalculateFraction(periodPortfolioPrice + liquidMoney, initPortfolioPrice);

        return (Math.Round(fraction, 4), Math.Round(periodPortfolioPrice, 4));
    }
}