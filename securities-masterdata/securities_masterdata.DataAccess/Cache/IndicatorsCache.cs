using System.Collections.Concurrent;
using securities_masterdata.DataAccess.Entities;
using TTM.Shared.Constants;

namespace securities_masterdata.DataAccess.Cache;

public struct IndicatorCache
{
    public Indicators IndicatorId { get; set; }
    public long SecurityId { get; set; }
    public DateOnly Date { get; set; }
    public decimal Value { get; set; }
}

public class IndicatorsCache
{
    private ConcurrentDictionary<long, Dictionary<Indicators, List<IndicatorCache>>> SecurityIndicators { get; } =
        new();

    public int UpdateCache(List<Indicator> indicators)
    {
        var updates = 0;
        foreach (var indicatorsGroup in indicators.GroupBy(i => i.SecurityId))
        {
            if (!SecurityIndicators.TryRemove(indicatorsGroup.Key, out var existingIndicators) &&
                existingIndicators != null) continue;

            SecurityIndicators[indicatorsGroup.Key] = ToIndicatorCacheDictionary(indicatorsGroup);
            updates++;
        }

        return updates;
    }

    public IEnumerable<Indicator> GetIndicatorsBySecurityId(long securityId,
        DateOnly? fromUtcDate = null, DateOnly? toUtcDate = null)
    {
        if (!SecurityIndicators.TryGetValue(securityId, out var indicators)) return [];

        if (fromUtcDate == null && toUtcDate == null) return FromIndicatorCacheDictionary(indicators);

        var indicatorsReturn = indicators.Values.SelectMany(list => list).AsEnumerable();
        if (fromUtcDate.HasValue)
            indicatorsReturn = indicatorsReturn.Where(i => i.Date >= fromUtcDate);

        if (toUtcDate.HasValue)
            indicatorsReturn = indicatorsReturn.Where(i => i.Date <= toUtcDate);

        return FromIndicatorCache(indicatorsReturn);
    }

    public IEnumerable<Indicator> GetIndicatorsBySecurityIds(HashSet<long> securityIds,
        DateOnly? fromUtcDate = null, DateOnly? toUtcDate = null)
    {
        return securityIds.SelectMany(securityId => GetIndicatorsBySecurityId(securityId, fromUtcDate, toUtcDate));
    }

    public IEnumerable<Indicator> GetIndicatorsByDate(DateOnly date, HashSet<long> indicatorIds,
        HashSet<long>? securityIds = null)
    {
        var targetIndicators = indicatorIds.Select(id => (Indicators)id).ToHashSet();

        if (securityIds != null)
        {
            return securityIds.SelectMany(securityId =>
            {
                if (!SecurityIndicators.TryGetValue(securityId, out var indicators)) return [];

                return targetIndicators
                    .Where(indicators.ContainsKey)
                    .SelectMany(indicatorType => indicators[indicatorType])
                    .Where(i => i.Date <= date)
                    .GroupBy(i => i.SecurityId)
                    .Select(group => group.OrderByDescending(i => i.Date).First())
                    .Select(FromIndicatorCache);
            });
        }

        return SecurityIndicators.SelectMany(si =>
            targetIndicators
                .Where(si.Value.ContainsKey)
                .SelectMany(indicatorType => si.Value[indicatorType])
                .Where(i => i.Date <= date)
                .GroupBy(i => i.SecurityId)
                .Select(group => group.OrderByDescending(i => i.Date).First())
                .Select(FromIndicatorCache)
        );
    }

    private static Dictionary<Indicators, List<IndicatorCache>> ToIndicatorCacheDictionary(
        IEnumerable<Indicator> indicators) =>
        indicators.GroupBy(i => i.IndicatorId)
            .ToDictionary(i => i.Key, i => i.Select(ii => new IndicatorCache
            {
                IndicatorId = ii.IndicatorId,
                SecurityId = ii.SecurityId,
                Date = ii.Date,
                Value = ii.Value
            }).ToList());

    private static IEnumerable<Indicator> FromIndicatorCache(IEnumerable<IndicatorCache> indicators) =>
        indicators.Select(i => new Indicator
        {
            IndicatorId = i.IndicatorId,
            SecurityId = i.SecurityId,
            Date = i.Date,
            Value = i.Value
        });

    private static Indicator FromIndicatorCache(IndicatorCache indicator) =>
        new()
        {
            IndicatorId = indicator.IndicatorId,
            SecurityId = indicator.SecurityId,
            Date = indicator.Date,
            Value = indicator.Value
        };

    private static IEnumerable<Indicator> FromIndicatorCacheDictionary(
        Dictionary<Indicators, List<IndicatorCache>> indicators) =>
        indicators.Values.SelectMany(FromIndicatorCache);
}