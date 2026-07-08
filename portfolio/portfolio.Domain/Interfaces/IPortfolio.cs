using portfolio.Domain.Models;
using TTM.Shared.Models.PortfolioSimulation;

namespace portfolio.Domain.Interfaces;

public interface IPortfolio
{
    Task<PortfolioDto> Compute(PortfolioInput input, string? inputHash = null, bool savePortfolio = true);
}