using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using Marten;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class SectorRepository : ISectorRepository
{
    private readonly IDocumentStore _store;

    public SectorRepository(IDocumentStore store)
    {
        _store = store;
    }

    public async Task Delete(string name, CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();
        session.DeleteWhere<Sector>(s => s.Name == name);
        await session.SaveChangesAsync(token);
    }

    public async Task<bool> Save(Sector sector, CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();

        var existing = await session.Query<Sector>()
            .FirstOrDefaultAsync(s => s.Name == sector.Name, token);
        if (existing is not null)
            sector.Id = existing.Id;

        session.Store(sector);
        await session.SaveChangesAsync(token);
        return true;
    }

    public async Task<long> SaveBatch(List<Sector> sectors, CancellationToken token = default)
    {
        if (sectors.Count == 0)
            return 0;

        await using var session = _store.LightweightSession();

        var names = sectors.Select(s => s.Name).ToList();
        var existing = await session.Query<Sector>()
            .Where(s => s.Name.IsOneOf(names))
            .ToListAsync(token);
        var idsByName = existing.ToDictionary(s => s.Name, s => s.Id);

        foreach (var sector in sectors)
        {
            if (idsByName.TryGetValue(sector.Name, out var id))
                sector.Id = id;
        }

        session.Store(sectors.ToArray());
        await session.SaveChangesAsync(token);
        return sectors.Count;
    }

    public async Task<Sector?> GetById(string name, CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        return await session.Query<Sector>()
            .FirstOrDefaultAsync(s => s.Name == name, token);
    }

    public async Task<IList<Sector>> GetAll(CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        var sectors = await session.Query<Sector>().ToListAsync(token);
        return sectors.ToList();
    }
}
