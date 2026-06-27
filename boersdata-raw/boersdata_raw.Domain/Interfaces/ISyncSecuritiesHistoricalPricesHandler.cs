using boersdata_raw.DataAccess.Models;

namespace boersdata_raw.Domain.Interfaces;

public interface ISyncSecuritiesHistoricalPricesHandler
{
    Task<List<string>> HandleSyncHistoricalPrices();
    Task HandleSelectedSyncHistoricalPrices(List<string> ticker);
}