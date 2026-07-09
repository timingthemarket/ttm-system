using TTM.Shared.Models.BoersDataRaw.Prices;

namespace boersdata_raw.Domain.Interfaces;

public interface ISyncSecuritiesDailyPricesHandler
{
    Task<List<SecurityPriceDto>> HandleDailyPricesSync();
}