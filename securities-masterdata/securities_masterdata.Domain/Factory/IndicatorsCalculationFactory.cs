using Microsoft.Extensions.Caching.Memory;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Factory;

public class IndicatorsCalculationFactory(IEnumerable<IFactoryFunction> functions, IMemoryCache memoryCache) : IIndicatorsCalculationFactory
{
    private readonly List<IFactoryFunction> _functions = functions.ToList();

    public async Task<List<SecurityIndicatorDto>> Compute(SecuritiesIndicatorQryMetadataDto indicator, List<Security> securities, DateOnly date)
    {
        var func = _functions.FirstOrDefault(f => f.Indicator == indicator.IndicatorId);
        if (func == null)
            return new List<SecurityIndicatorDto>();
        
        if (indicator.LookBackPeriod == null)
            return new List<SecurityIndicatorDto>();

        var cahceKey = $"{indicator.IndicatorId}-{indicator.LookBackPeriod.Period}-{indicator.LookBackPeriod.Aggregate}-{date}";
        if (memoryCache.TryGetValue(cahceKey, out List<SecurityIndicatorDto>? indicatorList) &&
            indicatorList is not null)
        {
            var securityIds = indicatorList.Select(s => s.SecurityId).ToHashSet();
            var allSecuritiesInList = securities.All(a => securityIds.Contains(a.SecurityId));
            if (allSecuritiesInList)
                return indicatorList;
        }
        
        var indicatorsReturn = await func.Process(securities, date, indicator.LookBackPeriod);

        memoryCache.Set(cahceKey, indicatorsReturn, TimeSpan.FromMinutes(30));

        return indicatorsReturn;
    }
}