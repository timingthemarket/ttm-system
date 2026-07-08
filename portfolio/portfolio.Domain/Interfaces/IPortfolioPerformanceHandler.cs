using portfolio.Domain.Models;

namespace portfolio.Domain.Interfaces;

public interface IPortfolioPerformanceHandler
{
    Task<PortfolioPerformanceResponse?> GetPerformanceBySetId(string setId, DateOnly? date);
}
