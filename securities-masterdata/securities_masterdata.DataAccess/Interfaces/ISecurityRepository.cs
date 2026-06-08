using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Entities.Composite;

namespace securities_masterdata.DataAccess.Interfaces;

public interface ISecurityRepository
{
    Task<List<Security>>  GetAll(bool includeInactive = false);
    Task<List<Security>> GetAllTracked();
    Task WriteMany(List<Security> securities);
    Task Update(Security security);
    Task UpdateAndReplaceAllSecurityPrices(long securityId, List<SecurityPrice> securityPrices, bool resetTracker = false);
    Task AddSecurityPrices(List<SecurityPrice> securityPrices);
    Task<List<SecurityPrice>> GetSecuritiesPricesHistory(HashSet<long> securityIds, DateOnly? fromUtcDate = null, DateOnly? toUtcDate = null);
    Task<List<SecurityPrice>> GetSecuritiesPricesByDate(DateOnly date, HashSet<long>? securitiesIds);
    Task<List<Security>> GetSecuritiesBySecurityIds(HashSet<long> securityIds, bool includeInactive = false);
    Task<List<Security>> GetSecuritiesByTickers(HashSet<string> tickers, bool noTracking = false);
    Task<List<long>> GetSecurityIdsByAverageVolume(double volumeLimit, DateOnly fromDate, DateOnly toDate);

    Task<List<SecurityPrice>> GetSecuritiesPricesHistoryNoCache(HashSet<long> securityIds, DateOnly? fromUtcDate = null,
        DateOnly? toUtcDate = null);
    Task UpdateInactiveStatus(List<long> securityIds, bool inactive);
}