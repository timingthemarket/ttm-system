using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using portfolio.DataAccess.Interfaces;
using portfolio.DataAccess.Models.Db;
using portfolio.Domain.Constants;
using portfolio.Domain.Extensions;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Models;
using portfolio.Domain.Queue;
using portfolio.Domain.Serialization;
using portfolio.Domain.Utils;
using TTM.Shared.Constants;
using TTM.Shared.Events;
using TTM.Shared.Events.PortfolioSimulation;
using TTM.Shared.Models.PortfolioSimulation;
using TTM.Shared.Models.PortfolioSimulation.Command;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;
using SimulationPeriod = portfolio.DataAccess.Models.Db.SimulationPeriod;


namespace portfolio.Domain.Handlers;

public class ProcessSimulationHandler(
    ILogger<ProcessSimulationHandler> logger,
    IPublishEndpoint publishEndpoint,
    SimulationQueueCache simulationQueue,
    IPortfolio portfolio,
    IMasterdataService masterdataService,
    ISimulationRepository simulationResultRepository)
    : IProcessSimulationHandler
{
    public async Task<SimulationDto?> HandleProcessSimulationFromQueue()
    {
        var simulationItem = simulationQueue.DequeueAndGetItem();
        if (simulationItem == null) return null;

        logger.LogInformation("Dequeued {Id}. Starting the simulation...", simulationItem.Id);

        return await ProcessSimulation(simulationItem);
    }

    public Task<SimulationDto> HandleProcessSimulationFromCmd(SimulationCmd cmd)
    {
        var processSim = new ProcessSimulation
        {
            Id = Guid.NewGuid(),
            RowSimilarityLimit = cmd.RowSimilarityLimit,
            RegistrationCreated = DateTime.UtcNow,
            InitMoney = cmd.InitMoney,
            DateSimulationEnd = cmd.DateSimulationEnd,
            Periods = cmd.Periods.Select(p => new Models.SimulationPeriod
            {
                StrategyId = p.StrategyId,
                DateStart = p.DateStart,
                MaxSecuritySpending = p.MaxSecuritySpending,
                Variables = p.Variables.Select(v => new SimulationFinancialVariable
                {
                    Direction = v.Direction,
                    IndicatorId = v.IndicatorId,
                    Weight = v.Weight,
                    LookBackPeriod = v.LookBackPeriod
                }).ToList()
            }).ToList()
        };

        return ProcessSimulation(processSim);
    }


    private async Task<SimulationDto> ProcessSimulation(ProcessSimulation simulationItem)
    {
        // Initially:
        // TODO: compare each period with how many periods the comparing index is beat
        // TODO: send event for every period that is completed

        var simResult = new Simulation
        {
            Id = simulationItem.Id,
            InitMoney = simulationItem.InitMoney,
            Registered = simulationItem.RegistrationCreated,
            PercentageChange = null
        };
        
        simulationResultRepository.SaveSimulation(simResult);
        
        var liquidMoney = simulationItem.InitMoney;
        var investedMoney = 0.0M;
        double portfolioValueIncrease = 0;
        PortfolioDto? previousPortfolio = null;
        foreach (Models.SimulationPeriod period in simulationItem.Periods.OrderBy(p => p.DateStart))
        {
            if (InternalSettings.Verbosity == LogVerbosity.All)
                logger.LogInformation("Calculating portfolio with variables: {Var}",
                    StringUtils.GetIndicatorsString(period.Variables));

            // Implement to base portfolio calculation on the init start of money
            var totalMoney = liquidMoney + investedMoney;
            var periodPortfolio = await GetPortfolio(period.DateStart, period.StrategyId,
                simulationItem.RowSimilarityLimit, period.Variables, totalMoney, period.MaxSecuritySpending,
                simulationItem.CountryWeight, simulationItem.SectorWeight);

            investedMoney = periodPortfolio.GetInvestedMoney();
            liquidMoney = totalMoney - investedMoney;
            if (previousPortfolio == null) // For the first portfolio, just assign it to the portfolio and move on
            {
                previousPortfolio = periodPortfolio;
                continue;
            }
            
            var previousPortfolioSecurityIds = previousPortfolio.PortfolioValues
                .Select(p => p.SecurityId).ToHashSet();
            var pricesAtNewDate =
                await masterdataService.GetLatestPrices(period.DateStart, previousPortfolioSecurityIds);
            
            // Get the difference from the previous portfolio, with the new prices
            (double Diff, double Fraction, double PortfolioValue) diff =
                CalculatePortfolioDifference((double)simulationItem.InitMoney, (double)liquidMoney, pricesAtNewDate.SecurityPrices,
                    previousPortfolio);
            investedMoney = (decimal)diff.PortfolioValue; // The new value of the invested portfolio

            portfolioValueIncrease = diff.Fraction;
            
            var dbPeriod = CreateSimulationPeriod(previousPortfolio, simResult, liquidMoney);
            
            simulationResultRepository.SaveSimulationPeriod(dbPeriod);
            previousPortfolio = periodPortfolio;
        }

        simResult.Completed = DateTime.UtcNow;
        simResult.PercentageChange = portfolioValueIncrease;
        
        simulationResultRepository.UpdateSimulation(simResult);

        simResult = await simulationResultRepository.GetSimulation(simResult.Id);
        
        return MakeSimulationDto(simResult!);
    }

    private SimulationDto MakeSimulationDto(Simulation simulation) => new()
    {
        InitMoney = simulation.InitMoney,
        Id = simulation.Id,
        Registered = simulation.Registered,
        PercentageChange = simulation.PercentageChange ?? 0,
        Completed = simulation.Completed,
        Periods = simulation.Periods.Select(p => new SimulationPeriodDto
        {
            Id = p.Id,
            LiquidMoney = p.LiquidMoney,
            InvestedMoney = p.InvestedMoney,
            Portfolio = new PortfolioDto
            {
                Id = p.Portfolio.Id,
                SecuritiesDate = p.Portfolio.SecuritiesDate,
                CalculationDate = p.Portfolio.CalculationDate,
                Strategy = p.Portfolio.Strategy,
                PortfolioValues = p.Portfolio.PortfolioValues
                    .Select(pv => new PortfolioValueDto
                    {
                        SecurityId = pv.SecurityId,
                        Weight = pv.Weight,
                        Rank = pv.Rank,
                        Amount = pv.Amount,
                        Price = pv.Price
                    }).ToList()
            }
        }).ToList()
    };

    private SimulationPeriod CreateSimulationPeriod(PortfolioDto portfolio, Simulation simulationItem, decimal liquidMoney)
    {
        return new SimulationPeriod
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id, 
            InvestedMoney = portfolio.GetInvestedMoney(),
            LiquidMoney = liquidMoney,
            SimulationId = simulationItem.Id
        };
    }

    private async Task<PortfolioDto> GetPortfolio(DateOnly date, long strategyId, double rowSimLimit,
        List<SimulationFinancialVariable> variables, decimal money, double maxSecuritySpending,
        Dictionary<string, double> countryWeights, Dictionary<string, double> sectorWeights)
    {
        var portfolioVariables = variables.Select(v => new PortfolioInputIndicatorVariable
        {
            Direction = v.Direction,
            IndicatorId = v.IndicatorId,
            Weight = v.Weight,
            LookBackPeriod = v.LookBackPeriod
        }).ToList();

        var input = new PortfolioInput
        {
            Date = date,
            RowSimilarity = rowSimLimit,
            StrategyId = strategyId,
            Indicators = portfolioVariables,
            CountryWeight = countryWeights,
            SectorWeight = sectorWeights,
            Money = money,
            MaxSecuritySpending = maxSecuritySpending
        };
        var portfolio1 = await portfolio.Compute(input, Functions.GetObjectHash(input, HashSerializer.Default.PortfolioInput));

        return portfolio1;
    }

    private (double Diff, double Fraction, double PeriodPortofolioPrice) CalculatePortfolioDifference(double initPortfolioPrice, double liquidMoney,
        List<SecurityPriceDto> newPortfolioPrices, PortfolioDto prevPortfolio)
    {
        // Join the previous portfolios outcome with the new prices
        var periodPortfolioPrice = prevPortfolio.PortfolioValues.Join(newPortfolioPrices, pv => pv.SecurityId, pp => pp.SecurityId, (
                portfolioValue, prices) => new { PortfolioValue = portfolioValue, Prices = prices })
            .Sum(join => join.Prices.MedianPrice() * join.PortfolioValue.Amount);
        
        var diff = periodPortfolioPrice + liquidMoney - initPortfolioPrice;
        var fraction = diff  / initPortfolioPrice; 
        
        return (Math.Round(diff, 4), Math.Round(fraction, 4), Math.Round(periodPortfolioPrice, 4));
    }
}