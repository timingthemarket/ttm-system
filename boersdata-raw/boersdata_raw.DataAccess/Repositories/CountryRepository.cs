using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using Marten;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class CountryRepository : ICountryRepository
{
    private readonly IDocumentStore _store;

    public CountryRepository(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<bool> Save(Country country, CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();

        var existing = await session.Query<Country>()
            .FirstOrDefaultAsync(c => c.Name == country.Name, token);
        if (existing is not null)
            country.Id = existing.Id;

        session.Store(country);
        await session.SaveChangesAsync(token);
        return true;
    }

    public async Task<long> SaveBatch(List<Country> countries, CancellationToken token = default)
    {
        if (countries.Count == 0)
            return 0;

        await using var session = _store.LightweightSession();

        var names = countries.Select(c => c.Name).ToList();
        var existing = await session.Query<Country>()
            .Where(c => c.Name.IsOneOf(names))
            .ToListAsync(token);
        var idsByName = existing.ToDictionary(c => c.Name, c => c.Id);

        foreach (var country in countries)
        {
            if (idsByName.TryGetValue(country.Name, out var id))
                country.Id = id;
        }

        session.Store(countries);
        await session.SaveChangesAsync(token);
        return countries.Count;
    }

    public async Task Delete(string name, CancellationToken token = default)
    {
        await using var session = _store.LightweightSession();
        session.DeleteWhere<Country>(c => c.Name == name);
        await session.SaveChangesAsync(token);
    }

    public async Task<Country?> GetById(string name, CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        return await session.Query<Country>()
            .FirstOrDefaultAsync(c => c.Name == name, token);
    }

    public async Task<IList<Country>> GetAll(CancellationToken token = default)
    {
        await using var session = _store.QuerySession();
        var countries = await session.Query<Country>().ToListAsync(token);
        return countries.ToList();
    }
}
