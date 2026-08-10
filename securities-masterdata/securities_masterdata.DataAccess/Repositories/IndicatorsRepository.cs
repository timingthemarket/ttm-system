using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using securities_masterdata.DataAccess.Cache;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using TTM.Shared.Constants;

namespace securities_masterdata.DataAccess.Repositories;

public class IndicatorsRepository(MasterdataDbContext dbContext, IMemoryCache cache, IndicatorsCache indicatorsCache) : IIndicatorsRepository
{
    public async Task<List<Indicator>> GetIndicatorsByDate(DateOnly date, HashSet<long> indicatorsIds,
        HashSet<long>? securityIds = null, bool useCache = true)
    {
        // Try to get data from cache first
        if (useCache)
        {
            var cachedData = indicatorsCache.GetIndicatorsByDate(date, indicatorsIds, securityIds).ToList();
        
            if (cachedData.Any())
            {
                // Filter cached data to get the latest indicators by date for each security
                var filteredCachedData = cachedData
                    .Where(i => i.Date <= date)
                    .GroupBy(i => i.SecurityId)
                    .Select(group => group.OrderByDescending(i => i.Date).First())
                    .ToList();

                if (filteredCachedData.Any())
                {
                    return filteredCachedData;
                }
            }
        }

        // Fallback to database query if cache is empty
        var dateString = date.ToString();
        var indicatorsIdsString = string.Join(",", indicatorsIds);
        
        var qry = @"
                    select i.* from indicators i INNER JOIN
                    (select security_id, MAX(idd.date) as max_date from indicators idd
                    WHERE idd.date <= '{0}' group by idd.security_id) ii
                    ON ii.security_id = i.security_id AND ii.max_date = i.date
                    where i.indicator_id in ({1})
                    ";
        
        var qryInput = string.Format(qry, dateString, indicatorsIdsString);
        
        if (securityIds != null)
        {
            var securityIdsjoin = string.Join(",", securityIds);
            qryInput += $" and i.security_id in ({securityIdsjoin})";
        }
        
        return await dbContext.Indicators.FromSqlRaw(qryInput).ToListAsync();
    }
    
    public async Task UpdateAndReplaceAllIndicators(long securityId, List<Indicator> indicators,
        bool resetTracker = false)
    {
        await DeleteIndicators(securityId);
        
        dbContext.Indicators.AddRange(indicators);
        await dbContext.SaveChangesAsync();
        if (resetTracker)
            dbContext.ChangeTracker.Clear();
    }
    
    public async Task<List<Indicator>> GetAggregatedIndicatorByDate(DateOnly date, Indicators indicatorId,
        Aggregator aggregator)
    {
        var cacheKey = $"GetAggregatedIndicatorByDate-{date}-{indicatorId}-{aggregator}";
        if (cache.TryGetValue(cacheKey, out List<Indicator>? indicatorsCache) && indicatorsCache != null)
            return indicatorsCache;
        
        var dataList = await dbContext.Indicators
            .Where(i => i.IndicatorId == indicatorId && i.Date <= date)
            .ToListAsync();
        var data = dataList.GroupBy(i => i.SecurityId);

        List<Indicator> indicators;
        switch (aggregator)
        {
            case Aggregator.Average:
                indicators = data.Select(i => new
                {
                    Indicator = i.OrderByDescending(ii => ii.Date).First(), Avg = i.Average(q => q.Value)
                }).Select(i => new Indicator
                {
                    IndicatorId = i.Indicator.IndicatorId,
                    SecurityId = i.Indicator.SecurityId,
                    Date = i.Indicator.Date,
                    Value = i.Avg
                }).ToList();
                break;
            case Aggregator.Sum:
                indicators = data.Select(i => new
                {
                    Indicator = i.OrderByDescending(ii => ii.Date).First(), Avg = i.Sum(q => q.Value)
                }).Select(i => new Indicator
                {
                    IndicatorId = i.Indicator.IndicatorId,
                    SecurityId = i.Indicator.SecurityId,
                    Date = i.Indicator.Date,
                    Value = i.Avg
                }).ToList();
                break;
            default:
                indicators = data.Select(i => new
                {
                    Indicator = i.OrderByDescending(ii => ii.Date).First(), Avg = i.OrderByDescending(q => q.Date)
                        .First().Value
                }).Select(i => new Indicator
                {
                    IndicatorId = i.Indicator.IndicatorId,
                    SecurityId = i.Indicator.SecurityId,
                    Date = i.Indicator.Date,
                    Value = i.Avg
                }).ToList();
                break;
        }

        return indicators;
    }
    
    public async Task<int> DeleteIndicators(long securityId) =>
        await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM indicators WHERE security_id = {securityId}");
}