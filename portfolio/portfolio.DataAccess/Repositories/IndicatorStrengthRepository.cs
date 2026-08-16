using Microsoft.EntityFrameworkCore;
using portfolio.DataAccess.Interfaces;
using portfolio.DataAccess.Models.Db;
using TTM.Shared.Constants;

namespace portfolio.DataAccess.Repositories;

public class IndicatorStrengthRepository : IIndicatorStrengthRepository
{
    public async Task SaveMany(DateOnly date, List<IndicatorStrength> strengths)
    {
        if (strengths.Count == 0) return;

        using var context = new PortfolioDbContext();
        await context.IndicatorStrengths
            .Where(s => s.Date == date)
            .ExecuteDeleteAsync();

        context.IndicatorStrengths.AddRange(strengths);
        await context.SaveChangesAsync();
    }

    public async Task<List<IndicatorStrength>> GetByDate(DateOnly date)
    {
        using var context = new PortfolioDbContext();
        return await context.IndicatorStrengths
            .AsNoTracking()
            .Where(s => s.Date == date)
            .OrderByDescending(s => s.SharpeRatio)
            .ToListAsync();
    }

    public async Task<List<IndicatorStrength>> GetByIndicator(Indicators indicator, Direction direction,
        DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        using var context = new PortfolioDbContext();
        var query = context.IndicatorStrengths
            .AsNoTracking()
            .Where(s => s.IndicatorId == indicator && s.Direction == direction);

        if (fromDate.HasValue)
            query = query.Where(s => s.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.Date <= toDate.Value);

        return await query.OrderBy(s => s.Date).ToListAsync();
    }

    public async Task<IndicatorStrength?> GetLatestForIndicator(Indicators indicator, Direction direction)
    {
        using var context = new PortfolioDbContext();
        return await context.IndicatorStrengths
            .AsNoTracking()
            .Where(s => s.IndicatorId == indicator && s.Direction == direction)
            .OrderByDescending(s => s.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<List<IndicatorStrength>> GetLatestForAllIndicators()
    {
        var latestDate = await GetLatestDate();
        if (latestDate == null) return new List<IndicatorStrength>();

        return await GetByDate(latestDate.Value);
    }

    public async Task<DateOnly?> GetLatestDate()
    {
        using var context = new PortfolioDbContext();
        var latest = await context.IndicatorStrengths
            .AsNoTracking()
            .OrderByDescending(s => s.Date)
            .FirstOrDefaultAsync();

        return latest?.Date;
    }

    public async Task DeleteByDate(DateOnly date)
    {
        using var context = new PortfolioDbContext();
        await context.IndicatorStrengths
            .Where(s => s.Date == date)
            .ExecuteDeleteAsync();
    }
}
