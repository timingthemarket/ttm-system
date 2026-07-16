using portfolio.DataAccess.Interfaces;
using portfolio.Domain.Interfaces;
using TTM.Shared.Models.PortfolioSimulation;

namespace portfolio.Domain.Handlers;

public class QuerySimulationsHandler(ISimulationRepository simulationRepository) : IQuerySimulationsHandler
{
    public List<SimulationDto> GetSimulations(int limit)
    {
        var simulations =  simulationRepository.GetSimulations(limit);

        return simulations.Select(s => new SimulationDto
        {
            Id = s.Id,
            Completed = s.Completed,
            Registered = s.Registered,
            InitMoney = s.InitMoney,
            PercentageChange = s.PercentageChange,
            Periods = s.Periods.Select(p => new SimulationPeriodDto
            {
                Id = p.Id,
                InvestedMoney = p.InvestedMoney,
                LiquidMoney = p.LiquidMoney,
                Portfolio = new PortfolioDto
                {
                    Id = p.Portfolio.Id,
                    SecuritiesDate = p.Portfolio.SecuritiesDate,
                    CalculationDate = p.Portfolio.CalculationDate,
                    Strategy = p.Portfolio.Strategy,
                    PortfolioValues = p.Portfolio.PortfolioValues.Select(pv => new PortfolioValueDto
                    {
                        SecurityId = pv.SecurityId,
                        Weight = pv.Weight,
                        Rank = pv.Rank,
                        Amount = pv.Amount,
                        Price = pv.Price
                    }).ToList()
                }
            }).ToList()
        }).ToList();
    }
}