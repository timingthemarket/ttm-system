using portfolio.DataAccess.Models.Db;

namespace portfolio.DataAccess.Interfaces;

public interface IPortfolioTrendsRepository
{
    Task SavePortfolioTrends(PortfolioTrends portfolioTrends);
    Task<PortfolioTrends?> GetPortfolioTrendsById(Guid id);
    Task<PortfolioTrends?> GetPortfolioTrendsBySetId(string setId);
    Task<int?> GetPortfolioTrensSetIdLatestRevision(string setId);
}