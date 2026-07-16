using Microsoft.EntityFrameworkCore;
using portfolio.DataAccess.Interfaces;
using portfolio.DataAccess.Models.Db;

namespace portfolio.DataAccess.Repositories;

public class PortfolioTrendsRepository : IPortfolioTrendsRepository
{
    public async Task SavePortfolioTrends(PortfolioTrends portfolioTrends)
    {
        using var context = new PortfolioDbContext();
        context.PortfolioTrends.Add(portfolioTrends);
        await context.SaveChangesAsync();
    }

    public async Task<PortfolioTrends?> GetPortfolioTrendsById(Guid id)
    {
        using var context = new PortfolioDbContext();
        return await context.PortfolioTrends
            .AsNoTracking()
            .Where(pt => pt.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<PortfolioTrends?> GetPortfolioTrendsBySetId(string setId)
    {
        using var context = new PortfolioDbContext();
        return await context.PortfolioTrends
            .AsNoTracking()
            .Where(pt => pt.SetId == setId)
            .FirstOrDefaultAsync();
    }
    
    public async Task<int?> GetPortfolioTrensSetIdLatestRevision(string setId)
    {
        using var context = new PortfolioDbContext();
        var set = await context.PortfolioTrends
            .AsNoTracking()
            .Where(pt => pt.SetId == setId)
            .OrderByDescending(pt => pt.Timestamp)
            .FirstOrDefaultAsync();
        
        return set?.Revision;
    }
}