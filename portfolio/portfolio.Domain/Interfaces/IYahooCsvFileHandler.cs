using TTM.Shared.Models.PortfolioSimulation;

namespace portfolio.Domain.Interfaces;

public interface IYahooCsvFileHandler
{
    Task<Stream> HandleMakeYahooCsvFile(PortfolioDto portfolioDto);
}