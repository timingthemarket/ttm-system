using Microsoft.EntityFrameworkCore;
using portfolio.DataAccess.Interfaces;
using portfolio.DataAccess.Models.Db;

namespace portfolio.DataAccess.Repositories;

public class PortfolioRepository : IPortfolioRepository
{
    public async Task SavePortfolio(Portfolio portfolio)
    {
        using var context = new PortfolioDbContext();
        context.Portfolios.Add(portfolio);
        await context.SaveChangesAsync();
    }
    
    public async Task<bool> CheckPortfolioWithHash(string hash)
    {
        using var context = new PortfolioDbContext();
        return await context.Portfolios.AnyAsync(p => p.Hash == hash);
    }
    
    public async Task<Portfolio?> GetPortfolioWithHash(string hash)
    {
        using var context = new PortfolioDbContext();
        return await context.Portfolios
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.Hash == hash)
            .Include(p => p.PortfolioValues)
            .Include(p => p.PortfolioIndicators)
            .FirstOrDefaultAsync();
    }

    public async Task<Portfolio?> GetPortfolioById(Guid portfolioId)
    {
        using var context = new PortfolioDbContext();
        return await context.Portfolios
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.Id == portfolioId)
            .Include(p => p.PortfolioValues)
            .Include(p => p.PortfolioIndicators)
            .FirstOrDefaultAsync();
    }

    public async Task<Portfolio?> GetPortfolioFromSimulationId(Guid simulationId)
    {
        using var context = new PortfolioDbContext();
        return await context.Portfolios
            .AsSplitQuery()
            .Where(p => p.SimulationPeriod.SimulationId == simulationId)
            .Include(p => p.PortfolioIndicators)
            .Include(p => p.PortfolioValues)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Portfolio>> GetPortfolioFromSimulationIds(List<Guid> simulationIds)
    {
        using var context = new PortfolioDbContext();
        return await context.Portfolios
            .AsNoTracking()
            .Where(p => simulationIds.Contains(p.SimulationPeriod.SimulationId))
            .Include(p => p.SimulationPeriod)
            .ThenInclude(s => s.Simulation)
            .Include(p => p.PortfolioIndicators)
            //.Include(p => p.PortfolioValues)
            .ToListAsync();
    }

    public async Task<Guid?> GetPortfolioIdBySetId(string setId)
    {
        using var context = new PortfolioDbContext();
        var result = await context.PortfolioOutcomeView
            .AsNoTracking()
            .Where(p => p.SetId == setId)
            .Select(p => p.PortfolioId)
            .FirstOrDefaultAsync();
        return result == Guid.Empty ? null : result;
    }
}