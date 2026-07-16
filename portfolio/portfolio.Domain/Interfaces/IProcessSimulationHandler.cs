using TTM.Shared.Models.PortfolioSimulation;
using TTM.Shared.Models.PortfolioSimulation.Command;

namespace portfolio.Domain.Interfaces;

public interface IProcessSimulationHandler
{
    Task<SimulationDto> HandleProcessSimulationFromCmd(SimulationCmd cmd);
    Task<SimulationDto?> HandleProcessSimulationFromQueue();
}