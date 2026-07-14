using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using Marten;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class MarketRepository : IMarketRepository
{
    private readonly IDocumentStore _store;

    public MarketRepository(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<bool> Save(Market market, CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();

        var existing = await session.Query<Market>()
            .FirstOrDefaultAsync(m => m.Name == market.Name, token);
        if (existing is not null)
            market.Id = existing.Id;

        session.Store(market);
        await session.SaveChangesAsync(token);
        return true;
    }

    public async Task<long> SaveBatch(List<Market> markets, CancellationToken token = default)
    {
        if (markets.Count == 0)
            return 0;

        await using var session = _store.LightweightSession();

        var names = markets.Select(m => m.Name).ToList();
        var existing = await session.Query<Market>()
            .Where(m => m.Name.IsOneOf(names))
            .ToListAsync(token);
        var idsByName = existing.ToDictionary(m => m.Name, m => m.Id);

        foreach (var market in markets)
        {
            if (idsByName.TryGetValue(market.Name, out var id))
                market.Id = id;
        }

        session.Store(markets);
        await session.SaveChangesAsync(token);
        return markets.Count;
    }

    public async Task Delete(string name, CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();
        session.DeleteWhere<Market>(m => m.Name == name);
        await session.SaveChangesAsync(token);
    }

    public async Task<Market?> GetById(string ticker, CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        return await session.Query<Market>()
            .FirstOrDefaultAsync(m => m.Name == ticker, token);
    }

    public async Task<IList<Market>> GetAll(CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        var markets = await session.Query<Market>().ToListAsync(token);
        return markets.ToList();
    }
}
