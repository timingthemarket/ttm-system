using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using securities_masterdata.DataAccess.Cache;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Entities.Composite;
using securities_masterdata.DataAccess.Interfaces;

namespace securities_masterdata.DataAccess.Repositories;

public class SecurityRepository(MasterdataDbContext dbContext, SecuritiesPricesCache cache) : ISecurityRepository
{
    public async Task WriteMany(List<Security> securities)
    {
        dbContext.Securities.AddRange(securities);
        await dbContext.SaveChangesAsync();
    }

    public async Task Update(Security security)
    {
        dbContext.Securities.Update(security);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<SecurityPrice>> GetSecuritiesPricesHistory(HashSet<long> securityIds,
        DateOnly? fromUtcDate = null, DateOnly? toUtcDate = null)
    {
        var securitiesToFetchFromDb = new HashSet<long>();
        var securityPricesReturn = new List<SecurityPrice>();
        foreach (var id in securityIds)
        {
            var prices = cache.GetSecuritiyPricesHistory(id, fromUtcDate, toUtcDate);
            if (prices != null && prices.Any())
            {
                securitiesToFetchFromDb.Add(id);
                securityPricesReturn.AddRange(prices);
            }
        }

        if (securitiesToFetchFromDb.Count == securityIds.Count) return securityPricesReturn;

        var securitiesToFetch = securityIds.Except(securitiesToFetchFromDb).ToHashSet();

        var allPrices = await GetSecuritiesPricesHistoryNoCache(securitiesToFetch, fromUtcDate, toUtcDate);
        
        return allPrices.Concat(securityPricesReturn).ToList();
    }
    
    public async Task<List<SecurityPrice>> GetSecuritiesPricesHistoryNoCache(HashSet<long> securityIds, DateOnly? fromUtcDate = null, DateOnly? toUtcDate = null)
    {
        var qry = dbContext.SecuritiesPrices
            .Where(s => securityIds.Contains(s.SecurityId)).AsNoTracking();
        
        if (fromUtcDate.HasValue && toUtcDate.HasValue)
        {
            return await qry
                .Where(s => s.Date >= fromUtcDate.Value && s.Date <= toUtcDate.Value)
                .ToListAsync();
        }

        if (fromUtcDate.HasValue)
        {
            return await qry
                .Where(s => s.Date >= fromUtcDate)
                .ToListAsync();
        }

        return await qry.ToListAsync();
    }
    
    public async Task<List<SecurityPrice>> GetSecuritiesPricesByDate(DateOnly date, HashSet<long>? securitiesIds)
    {
        var dateString = date.ToString();
        
        var qry = $"""
                   select sp.* from securities_prices sp INNER JOIN
                   (select spp.security_id, MAX(spp.date) as max_date from securities_prices spp
                   WHERE spp.date <= '{dateString}' group by spp.security_id) iSp
                   ON iSp.security_id = sp.security_id AND iSp.max_date = sp.date
                   """;
        
        var cachedData = cache.GetSecuritiesPrices();
        var filteredCachedData = new List<SecurityPrice>();
        
        if (securitiesIds != null && securitiesIds.Any())
        {
            var securitiesIdsString = string.Join(",", securitiesIds);
            qry += $"\n where sp.security_id in ({securitiesIdsString})";
            
            filteredCachedData = cachedData
                .Where(c => securitiesIds.Contains(c.SecurityId))
                .Where(sp => sp.Date <= date)
                .GroupBy(sp => sp.SecurityId)
                .Select(group => group.OrderByDescending(sp => sp.Date).First())
                .ToList();

            // Check that all requested securities are present in the cached data
            var cachedSecurityIds = filteredCachedData.Select(sp => sp.SecurityId).ToHashSet();
            if (securitiesIds.All(id => cachedSecurityIds.Contains(id)))
            {
                return filteredCachedData;
            }
        }
        else
        {
            filteredCachedData = cachedData
                .Where(sp => sp.Date <= date)
                .GroupBy(sp => sp.SecurityId)
                .Select(group => group.OrderByDescending(sp => sp.Date).First())
                .ToList();
        }

        if (filteredCachedData.Any())
        {
            // If we have cached data, return it
            return filteredCachedData;
        }
        
        return await dbContext.SecuritiesPrices.FromSqlRaw(qry).ToListAsync();
    }
    
    public async Task UpdateAndReplaceAllSecurityPrices(long securityId, List<SecurityPrice> securityPrices, bool resetTracker = false)
    {
        await DeleteSecurityPrices(securityId);

        dbContext.SecuritiesPrices.AddRange(securityPrices);

        await dbContext.SaveChangesAsync();
        if (resetTracker)
            dbContext.ChangeTracker.Clear();
    }

    public async Task AddSecurityPrices(List<SecurityPrice> securityPrices)
    {
        if (!securityPrices.Any()) return;

        var securityIds = securityPrices.Select(sp => sp.SecurityId).ToHashSet();
        var dates = securityPrices.Select(sp => sp.Date).ToHashSet();
        
        var existingPrices = await dbContext.SecuritiesPrices
            .Where(sp => securityIds.Contains(sp.SecurityId) && dates.Contains(sp.Date))
            .Select(sp => new { sp.SecurityId, sp.Date })
            .ToListAsync();
        
        var existingKeys = existingPrices.ToHashSet();
        
        var pricesToAdd = securityPrices
            .Where(sp => !existingKeys.Contains(new { sp.SecurityId, sp.Date }))
            .ToList();
        
        if (pricesToAdd.Any())
        {
            dbContext.SecuritiesPrices.AddRange(pricesToAdd);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<int> DeleteSecurityPrices(long securityId)
    {
        return await dbContext.Database.ExecuteSqlAsync($"DELETE FROM securities_prices WHERE security_id = {securityId}");
    }

    public async Task<List<Security>> GetSecuritiesByTickers(HashSet<string> tickers, bool noTracking = false)
    {
        var qry = dbContext.Securities
            .Include(s => s.Currency)
            .Include(s => s.Market)
            .Where(s => tickers.Contains(s.Ticker) && !s.Market.Inactive);

        if (noTracking)
            qry = qry.AsNoTracking();
        
        return await qry.ToListAsync();
    }
    
    public async Task<List<Security>> GetSecuritiesBySecurityIds(HashSet<long> securityIds, bool includeInactive = false)
    {
        var qry = dbContext.Securities
            .Include(s => s.Currency)
            .Include(s => s.Market)
            .AsNoTracking()
            .Where(s => securityIds.Contains(s.SecurityId) && !s.Market.Inactive);
        
        if (!includeInactive)
            qry = qry.Where(s => !s.Inactive);
        
        return await qry.ToListAsync();
    }

    public async Task<List<Security>> GetAll(bool includeInactive = false)
    {
        var query = dbContext.Securities
            .Include(s => s.Currency)
            .Include(s => s.Market)
            .AsNoTracking();
        
        if (!includeInactive)
            query = query.Where(s => !s.Inactive).Where(s => !s.Market.Inactive);
        
        return await query
            .ToListAsync();
    }

    public async Task<List<Security>> GetAllTracked()
    {
        return await dbContext.Securities
            .ToListAsync();
    }

    public async Task<List<long>> GetSecurityIdsByAverageVolume(double volumeLimit, DateOnly fromDate, DateOnly toDate)
    {
        return await dbContext.SecuritiesPrices.AsNoTracking()
            .Where(sp => sp.Date <= toDate && sp.Date >= fromDate)
            .GroupBy(sp => sp.SecurityId)
            .Select(sp => new { SecurityId = sp.Key, AverageVolume = sp.Select(p => p.Volume).Average() })
            .Where(sp => sp.AverageVolume > volumeLimit)
            .Select(sp => sp.SecurityId)
            .ToListAsync();
    }

    public async Task UpdateInactiveStatus(List<long> securityIds, bool inactive)
    {
        if (!securityIds.Any()) return;

        var securityIdsString = string.Join(",", securityIds);
        var sql = $"UPDATE securities SET inactive = {inactive}, updated = CURRENT_TIMESTAMP WHERE security_id IN ({securityIdsString})";
        
        await dbContext.Database.ExecuteSqlRawAsync(sql);
    }
}