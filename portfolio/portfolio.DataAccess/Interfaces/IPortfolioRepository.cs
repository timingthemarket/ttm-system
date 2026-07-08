using portfolio.DataAccess.Models;
using portfolio.DataAccess.Models.Db;

namespace portfolio.DataAccess.Interfaces;

public interface IPortfolioRepository
{
    Task SavePortfolio(Portfolio portfolio);
    Task<Portfolio?> GetPortfolioWithHash(string hash);
    Task<Portfolio?> GetPortfolioById(Guid portfolioId);
    Task<Portfolio?> GetPortfolioFromSimulationId(Guid simulationId);
    Task<List<Portfolio>> GetPortfolioFromSimulationIds(List<Guid> simulationIds);
    Task<bool> CheckPortfolioWithHash(string hash);
    Task<Guid?> GetPortfolioIdBySetId(string setId);
}