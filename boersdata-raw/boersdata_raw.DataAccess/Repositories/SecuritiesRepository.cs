using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using Marten;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class SecuritiesRepository : ISecuritiesRepository
{
    private readonly IDocumentStore _store;

    public SecuritiesRepository(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<bool> Save(Security security, CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();
        security.Origin = SecurityOrigin.Nordic;

        var existing = await session.Query<Security>()
            .FirstOrDefaultAsync(s => s.Origin == SecurityOrigin.Nordic && s.Ticker == security.Ticker, token);
        if (existing is not null)
            security.Id = existing.Id;

        session.Store(security);
        await session.SaveChangesAsync(token);
        return true;
    }

    public async Task<long> SaveBatch(List<Security> security, CancellationToken token = default)
    {
        if (security.Count == 0)
            return 0;

        await using var session = _store.LightweightSession();

        var tickers = security.Select(s => s.Ticker).ToList();
        var existing = await session.Query<Security>()
            .Where(s => s.Origin == SecurityOrigin.Nordic && s.Ticker.IsOneOf(tickers))
            .ToListAsync(token);
        var idsByTicker = existing.ToDictionary(s => s.Ticker, s => s.Id);

        foreach (var item in security)
        {
            item.Origin = SecurityOrigin.Nordic;
            if (idsByTicker.TryGetValue(item.Ticker, out var id))
                item.Id = id;
        }

        session.Store(security.ToArray());
        await session.SaveChangesAsync(token);
        return security.Count;
    }

    public async Task<long> SaveGlobalBatch(List<Security> security, CancellationToken token = default)
    {
        if (security.Count == 0)
            return 0;

        await using var session = _store.LightweightSession();

        foreach (var item in security)
            item.Origin = SecurityOrigin.Global;

        session.Store(security.ToArray());
        await session.SaveChangesAsync(token);
        return security.Count;
    }

    public async Task DeleteBatch(List<long> insIds, CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();
        session.DeleteWhere<Security>(s => s.Origin == SecurityOrigin.Nordic && s.InsId.IsOneOf(insIds));
        await session.SaveChangesAsync(token);
    }

    public async Task DeleteAllNordic(CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();
        session.DeleteWhere<Security>(s => s.Origin == SecurityOrigin.Nordic);
        await session.SaveChangesAsync(token);
    }

    public async Task DeleteAllGlobal(CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();
        session.DeleteWhere<Security>(s => s.Origin == SecurityOrigin.Global);
        await session.SaveChangesAsync(token);
    }

    public async Task Delete(string ticker, CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();
        session.DeleteWhere<Security>(s => s.Origin == SecurityOrigin.Nordic && s.Ticker == ticker);
        await session.SaveChangesAsync(token);
    }

    public async Task<Security?> GetById(string ticker, CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        return await session.Query<Security>()
            .FirstOrDefaultAsync(s => s.Origin == SecurityOrigin.Nordic && s.Ticker == ticker, token);
    }

    public async Task<List<Security>> GetNordicSecurities(int? limit = null, CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        var query = session.Query<Security>()
            .Where(s => s.Origin == SecurityOrigin.Nordic);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        var securities = await query.ToListAsync(token);
        return securities.ToList();
    }

    public async Task<List<Security>> GetGlobalSecurities(int? limit = null, CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        var query = session.Query<Security>()
            .Where(s => s.Origin == SecurityOrigin.Global);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        var securities = await query.ToListAsync(token);
        return securities.ToList();
    }

    public async Task<List<Security>> GetAllSecurities(int? limit = null, CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        IQueryable<Security> query = session.Query<Security>();

        if (limit.HasValue)
            query = query.Take(limit.Value);

        var securities = await query.ToListAsync(token);
        return securities.ToList();
    }

    public async Task<List<Security>> GetStockTypeSecurities(CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        var securities = await session.Query<Security>()
            .Where(s => s.Type == SecurityType.Stocks || s.Type == SecurityType.Adr)
            .ToListAsync(token);
        return securities.ToList();
    }

    public async Task<List<Security>> GetNordicSecurities(List<string> securitiesTickers, CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        var securities = await session.Query<Security>()
            .Where(s => s.Origin == SecurityOrigin.Nordic && s.Ticker.IsOneOf(securitiesTickers))
            .ToListAsync(token);
        return securities.ToList();
    }

    public async Task<List<Security>> GetGlobalSecurities(List<string> securitiesTickers,
        CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        var securities = await session.Query<Security>()
            .Where(s => s.Origin == SecurityOrigin.Global && s.Ticker.IsOneOf(securitiesTickers))
            .ToListAsync(token);
        return securities.ToList();
    }
}
