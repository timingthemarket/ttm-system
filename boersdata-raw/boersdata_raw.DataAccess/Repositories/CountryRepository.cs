using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class CountryRepository : ICountryRepository
{
    public async Task<bool> Save(Country country, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(token);

        await context.Database.ExecuteSqlAsync($"DELETE FROM country WHERE name = {country.Name}", token);

        country.Id = 0;
        context.Countries.Add(country);
        await context.SaveChangesAsync(token);
        await transaction.CommitAsync(token);

        return true;
    }

    public async Task<long> SaveBatch(List<Country> countries, CancellationToken token = default)
    {
        if (countries.Count == 0)
            return 0;

        await using var context = new BoersDataDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(token);

        var names = countries.Select(c => c.Name).ToArray();
        await context.Database.ExecuteSqlAsync($"DELETE FROM country WHERE name = ANY({names})", token);

        foreach (var country in countries)
            country.Id = 0;

        context.Countries.AddRange(countries);
        await context.SaveChangesAsync(token);
        await transaction.CommitAsync(token);

        return countries.Count;
    }

    public async Task Delete(string name, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        await context.Database.ExecuteSqlAsync($"DELETE FROM country WHERE name = {name}", token);
    }

    public async Task<Country?> GetById(string name, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        return await context.Countries.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name, token);
    }

    public async Task<IList<Country>> GetAll(CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        return await context.Countries.AsNoTracking().ToListAsync(token);
    }
}
