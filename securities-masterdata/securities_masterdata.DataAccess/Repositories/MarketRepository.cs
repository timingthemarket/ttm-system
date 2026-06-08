using Microsoft.EntityFrameworkCore;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;

namespace securities_masterdata.DataAccess.Repositories;

public class MarketRepository : IMarketRepository
{
    private readonly MasterdataDbContext _dbContext;

    public MarketRepository(MasterdataDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Market>> UpdateAllMarkets(List<Market> markets)
    {
        var marketsDb = await _dbContext.Markets.ToListAsync();
        
        var newMarkets = markets
            .Where(m => !marketsDb.Select(mm => mm.Name).Contains(m.Name))
            .ToList();

        if (newMarkets.Any())
        {
            _dbContext.Markets.AddRange(newMarkets);
            await _dbContext.SaveChangesAsync();
        }
        
        marketsDb.AddRange(newMarkets);
        return marketsDb;
    }
    
    public async Task<List<Market>> GetAllMarkets() => await _dbContext.Markets.ToListAsync();
}