using boersdata_raw.DataAccess.Models;

namespace boersdata_raw.DataAccess.Interfaces;

public interface IStockPricesRepository
{
    public Task<List<StockPrice>> GetHistoricalPrices(string ticker, CancellationToken token = default);

    public Task<List<StockPrice>> GetHistoricalPrices(string ticker, DateTime fromDate,
        CancellationToken token = default);

    public Task<bool> SavePrice(StockPrice price, CancellationToken token = default);
    public Task<int> SaveHistoricalPrices(List<StockPrice> prices, string? ticker, bool useCache = true,
        CancellationToken token = default);
    Task<int> SaveBatch(List<StockPrice> prices, CancellationToken token = default);
    Task<int> OverwriteHistoricalPrices(string ticker, List<StockPrice> prices);
}