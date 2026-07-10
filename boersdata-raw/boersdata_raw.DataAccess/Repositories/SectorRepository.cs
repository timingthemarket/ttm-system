using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class SectorRepository : ISectorRepository
{
    public async Task Delete(string name, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        // fk_industry_sector cascades, removing the sector's industry rows
        await context.Database.ExecuteSqlAsync($"DELETE FROM sector WHERE name = {name}", token);
    }

    public async Task<bool> Save(Sector sector, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(token);

        await context.Database.ExecuteSqlAsync($"DELETE FROM sector WHERE name = {sector.Name}", token);

        sector.Id = 0;
        context.Sectors.Add(sector);
        await context.SaveChangesAsync(token);
        await transaction.CommitAsync(token);

        return true;
    }

    public async Task<long> SaveBatch(List<Sector> sectors, CancellationToken token = default)
    {
        if (sectors.Count == 0)
            return 0;

        await using var context = new BoersDataDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(token);

        var names = sectors.Select(s => s.Name).ToArray();
        await context.Database.ExecuteSqlAsync($"DELETE FROM sector WHERE name = ANY({names})", token);

        foreach (var sector in sectors)
            sector.Id = 0;

        context.Sectors.AddRange(sectors);
        await context.SaveChangesAsync(token);
        await transaction.CommitAsync(token);

        return sectors.Count;
    }

    public async Task<Sector?> GetById(string name, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        return await context.Sectors.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Name == name, token);
    }

    public async Task<IList<Sector>> GetAll(CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        return await context.Sectors.AsNoTracking().ToListAsync(token);
    }
}
