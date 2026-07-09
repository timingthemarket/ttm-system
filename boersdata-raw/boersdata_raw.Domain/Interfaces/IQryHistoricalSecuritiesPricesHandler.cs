
using TTM.Shared.Models.BoersDataRaw.Prices;

namespace boersdata_raw.Domain.Interfaces;

public interface IQryHistoricalSecuritiesPricesHandler
{
    Task<List<HistoricalPricesDto>> HandleGetHistoricalPrices(List<string> tickers);
}