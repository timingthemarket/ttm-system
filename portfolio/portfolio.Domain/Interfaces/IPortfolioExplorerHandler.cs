using portfolio.Domain.Models;
using portfolio.Domain.Services;

namespace portfolio.Domain.Interfaces;

public interface IPortfolioExplorerHandler
{
    Task<bool> HandlePortfolioDiscover(int sessionId, DateOnly sessionDate, List<PortfolioInputIndicatorVariable> indicators,
        HashSet<string> portfolioHashes, int initMoney, CancellationToken cancellationToken = default);
}