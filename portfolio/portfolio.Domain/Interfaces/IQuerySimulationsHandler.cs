using TTM.Shared.Models.PortfolioSimulation;

namespace portfolio.Domain.Interfaces;

public interface IQuerySimulationsHandler
{
    List<SimulationDto> GetSimulations(int limit);
}