using TTM.Shared.Models.PortfolioSimulation;
using TTM.Shared.Models.PortfolioSimulation.Command;

namespace portfolio.Domain.Interfaces;

public interface IRegisterSimulationHandler
{
    Task<RegisterSimulationDto> HandleRegisterSimulation(SimulationCmd evt);
}