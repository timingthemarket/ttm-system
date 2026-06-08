using securities_masterdata.DataAccess.Entities;
using TTM.Shared.Constants;

namespace securities_masterdata.DataAccess.Interfaces;

public interface IIndicatorsRepository
{
    Task UpdateAndReplaceAllIndicators(long securityId, List<Indicator> indicators, bool resetTracker = false);
    Task<List<Indicator>> GetIndicatorsByDate(DateOnly date, HashSet<long> indicatorsId,
        HashSet<long>? securityIds = null);

    Task<List<Indicator>> GetAggregatedIndicatorByDate(DateOnly date, Indicators indicatorId, Aggregator aggregator);
}