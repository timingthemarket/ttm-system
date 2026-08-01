using System.Collections.Concurrent;
using securities_masterdata.DataAccess.Entities;

namespace securities_masterdata.DataAccess.Cache;

public struct SecurityPriceCache
{
    public long SecurityId { get; set; }
    public DateOnly Date { get; set; }
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public long Volume { get; set; }
}

public record SecurityPriceCacheWrapper(DateTime DatePricesUpdated, List<SecurityPriceCache> Prices);


public class SecuritiesPricesCache
{
    private ConcurrentDictionary<long, SecurityPriceCacheWrapper> SecurityPrices { get; set; } = new();

    public bool IsEmpty => SecurityPrices.IsEmpty;

    public int UpdateCache(List<SecurityPrice> securityPrices)
    {
        int updates = 0;
        foreach (var pricesGroup in securityPrices.GroupBy(p => p.SecurityId))
        {
            SecurityPrices[pricesGroup.Key] = new SecurityPriceCacheWrapper(DateTime.UtcNow, ToSecurityPriceCache(pricesGroup));
            updates++;
        }

        return updates;
    }
    
    public IEnumerable<SecurityPrice> GetSecuritiyPricesHistory(long securityId,
        DateOnly? fromUtcDate = null, DateOnly? toUtcDate = null)
    {
        if (!SecurityPrices.TryGetValue(securityId, out var wrapper)) return [];

        if (fromUtcDate == null && toUtcDate == null) return FromSecurityPriceCache(wrapper.Prices);

        var pricesReturn = wrapper.Prices.AsEnumerable();
        if (fromUtcDate.HasValue)
            pricesReturn = pricesReturn.Where(p => p.Date >= fromUtcDate);
        
        if (toUtcDate.HasValue)
            pricesReturn = pricesReturn.Where(p => p.Date <= toUtcDate);

        return FromSecurityPriceCache(pricesReturn);
    }
    
    public IEnumerable<SecurityPrice> GetSecuritiesPricesHistoryByIds(HashSet<long> securityIds,
        DateOnly? fromUtcDate = null, DateOnly? toUtcDate = null)
    {
        return securityIds.SelectMany(securityId => GetSecuritiyPricesHistory(securityId, fromUtcDate, toUtcDate));
    }

    public IEnumerable<SecurityPrice> GetSecuritiesPrices(
        DateOnly? fromUtcDate = null, DateOnly? toUtcDate = null)
    {
        return SecurityPrices.SelectMany(sp => GetSecuritiyPricesHistory(sp.Key, fromUtcDate, toUtcDate));
    }
    
    private static List<SecurityPriceCache> ToSecurityPriceCache(IEnumerable<SecurityPrice> securityPrices) =>
        securityPrices.Select(p => new SecurityPriceCache
        {
            SecurityId = p.SecurityId,
            Date = p.Date,
            Open = p.Open,
            High = p.High,
            Low = p.Low,
            Close = p.Close,
            Volume = p.Volume
        }).ToList();

    private static IEnumerable<SecurityPrice> FromSecurityPriceCache(IEnumerable<SecurityPriceCache> securityPrices) =>
        securityPrices.Select(p => new SecurityPrice
        {
            SecurityId = p.SecurityId,
            Date = p.Date,
            Open = p.Open,
            High = p.High,
            Low = p.Low,
            Close = p.Close,
            Volume = p.Volume
        });
}