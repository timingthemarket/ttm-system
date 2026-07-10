using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class SecuritiesRepository : ISecuritiesRepository
{
    public async Task<bool> Save(Security security, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(token);

        await context.Securities
            .Where(s => s.Origin == SecurityOrigin.Nordic && s.Ticker == security.Ticker)
            .ExecuteDeleteAsync(token);

        security.Id = 0;
        security.Origin = SecurityOrigin.Nordic;
        context.Securities.Add(security);
        await context.SaveChangesAsync(token);
        await transaction.CommitAsync(token);

        return true;
    }

    public async Task<long> SaveBatch(List<Security> security, CancellationToken token = default)
    {
        if (security.Count == 0)
            return 0;

        await using var context = new BoersDataDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(token);

        var tickers = security.Select(s => s.Ticker).ToList();
        await context.Securities
            .Where(s => s.Origin == SecurityOrigin.Nordic && tickers.Contains(s.Ticker))
            .ExecuteDeleteAsync(token);

        foreach (var item in security)
        {
            item.Id = 0;
            item.Origin = SecurityOrigin.Nordic;
        }

        context.Securities.AddRange(security);
        await context.SaveChangesAsync(token);
        await transaction.CommitAsync(token);

        return security.Count;
    }

    public async Task<long> SaveGlobalBatch(List<Security> security, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();

        foreach (var item in security)
        {
            item.Id = 0;
            item.Origin = SecurityOrigin.Global;
        }

        context.Securities.AddRange(security);
        await context.SaveChangesAsync(token);

        return security.Count;
    }

    public async Task DeleteBatch(List<long> insIds, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        await context.Securities
            .Where(s => s.Origin == SecurityOrigin.Nordic && insIds.Contains(s.InsId))
            .ExecuteDeleteAsync(token);
    }

    public async Task DeleteAllNordic(CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        await context.Securities
            .Where(s => s.Origin == SecurityOrigin.Nordic)
            .ExecuteDeleteAsync(token);
    }

    public async Task DeleteAllGlobal(CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        await context.Securities
            .Where(s => s.Origin == SecurityOrigin.Global)
            .ExecuteDeleteAsync(token);
    }

    public async Task Delete(string ticker, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        await context.Securities
            .Where(s => s.Origin == SecurityOrigin.Nordic && s.Ticker == ticker)
            .ExecuteDeleteAsync(token);
    }

    public async Task<Security?> GetById(string ticker, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        return await context.Securities.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Origin == SecurityOrigin.Nordic && s.Ticker == ticker, token);
    }

    public async Task<List<Security>> GetNordicSecurities(int? limit = null, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        var query = context.Securities.AsNoTracking()
            .Where(s => s.Origin == SecurityOrigin.Nordic);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return await query.ToListAsync(token);
    }

    public async Task<List<Security>> GetGlobalSecurities(int? limit = null, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        var query = context.Securities.AsNoTracking()
            .Where(s => s.Origin == SecurityOrigin.Global);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return await query.ToListAsync(token);
    }

    public async Task<List<Security>> GetAllSecurities(int? limit = null, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        var query = context.Securities.AsNoTracking().AsQueryable();

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return await query.ToListAsync(token);
    }

    public async Task<List<Security>> GetStockTypeSecurities(CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        return await context.Securities.AsNoTracking()
            .Where(s => s.Type == SecurityType.Stocks || s.Type == SecurityType.Adr)
            .ToListAsync(token);
    }

    public async Task<List<Security>> GetNordicSecurities(List<string> securitiesTickers, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        return await context.Securities.AsNoTracking()
            .Where(s => s.Origin == SecurityOrigin.Nordic && securitiesTickers.Contains(s.Ticker))
            .ToListAsync(token);
    }

    public async Task<List<Security>> GetGlobalSecurities(List<string> securitiesTickers,
        CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        return await context.Securities.AsNoTracking()
            .Where(s => s.Origin == SecurityOrigin.Global && securitiesTickers.Contains(s.Ticker))
            .ToListAsync(token);
    }
}
