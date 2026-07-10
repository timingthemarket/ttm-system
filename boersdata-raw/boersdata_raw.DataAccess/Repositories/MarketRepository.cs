using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace boersdata_raw.DataAccess.Repositories;

public sealed class MarketRepository : IMarketRepository
{
    public async Task<bool> Save(Market market, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(token);

        await context.Database.ExecuteSqlAsync($"DELETE FROM market WHERE name = {market.Name}", token);

        market.Id = 0;
        context.Markets.Add(market);
        await context.SaveChangesAsync(token);
        await transaction.CommitAsync(token);

        return true;
    }

    public async Task<long> SaveBatch(List<Market> markets, CancellationToken token = default)
    {
        if (markets.Count == 0)
            return 0;

        await using var context = new BoersDataDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(token);

        var names = markets.Select(m => m.Name).ToArray();
        await context.Database.ExecuteSqlAsync($"DELETE FROM market WHERE name = ANY({names})", token);

        foreach (var market in markets)
            market.Id = 0;

        context.Markets.AddRange(markets);
        await context.SaveChangesAsync(token);
        await transaction.CommitAsync(token);

        return markets.Count;
    }

    public async Task Delete(string name, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        await context.Database.ExecuteSqlAsync($"DELETE FROM market WHERE name = {name}", token);
    }

    public async Task<Market?> GetById(string ticker, CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        return await context.Markets.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Name == ticker, token);
    }

    public async Task<IList<Market>> GetAll(CancellationToken token = default)
    {
        await using var context = new BoersDataDbContext();
        return await context.Markets.AsNoTracking().ToListAsync(token);
    }
}
