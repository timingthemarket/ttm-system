using Microsoft.EntityFrameworkCore;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using Index = securities_masterdata.DataAccess.Entities.Index;

namespace securities_masterdata.DataAccess.Repositories;

public class IndexRepository : IIndexRepository
{
    private readonly MasterdataDbContext _dbContext;

    public IndexRepository(MasterdataDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InsertIndexValues(List<IndexValue> values)
    {
        _dbContext.IndexValues.AddRange(values);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Index>> GetIndexWithSecurities(bool asNoTracking = true)
    {
        var qry = _dbContext.Indexes
            .Include(i => i.IndexSecurities)
            .AsQueryable();

        if (asNoTracking)
        {
            qry = qry.AsNoTracking();
        }
        
        return await qry.ToListAsync();
    }

    public async Task<List<IndexValue>> GetLatestIndexValues()
    {
        var qry = $"""
                   select sp.* from index_values sp INNER JOIN
                   (select spp.index_id, MAX(spp.date) as max_date from index_values spp
                    group by spp.index_id) iSp
                   ON iSp.index_id = sp.index_id AND iSp.max_date = sp.date
                   """;
        return await _dbContext.IndexValues.FromSqlRaw(qry).ToListAsync();
    }

    public async Task<Index?> GetIndexById(long indexId)
    {
        return await _dbContext.Indexes
            .Include(i => i.IndexSecurities)
            .SingleOrDefaultAsync(i => i.IndexId == indexId);
    }

    public async Task<List<IndexValue>> GetIndexValues(long indexId, DateOnly? fromUtcDate = null,
        DateOnly? toUtcDate = null)
    {
        var qry = _dbContext.IndexValues.Where(i => i.IndexId == indexId).AsNoTracking();

        if (fromUtcDate.HasValue && toUtcDate.HasValue)
        {
            qry = qry.Where(q => q.Date >= fromUtcDate.Value && q.Date <= toUtcDate.Value);
        } else if (fromUtcDate.HasValue)
        {
            qry = qry.Where(q => q.Date > fromUtcDate.Value);
        }
        
        return await qry.ToListAsync();
    }

    public async Task<int> DeleteIndexValues(long indexId)
    {
        return await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM index_values WHERE index_id = {indexId}");
    }
    
    public async Task SaveIndex(Index index)
    {
        _dbContext.Update(index);
        await _dbContext.SaveChangesAsync();
    }
}