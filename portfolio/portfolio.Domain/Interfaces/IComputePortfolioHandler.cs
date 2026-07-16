using portfolio.Domain.Models;
using TTM.Shared.Models.PortfolioSimulation;

namespace portfolio.Domain.Interfaces;

public interface IComputePortfolioHandler
{
    Task<SecuritiesPortfolioQryResponse> HandleComputePortfolio(DateOnly date, long strategyId, double rowSimilarity,
        List<PortfolioInputIndicatorVariable> variables, decimal money, double maxSecuritySpending);
}