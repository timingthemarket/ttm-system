using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using boersdata_raw.DataAccess.Models.BoersDataApi;
using boersdata_raw.Domain.Interfaces;
using boersdata_raw.Domain.Models;
using boersdata_raw.Domain.Queue;
using Microsoft.Extensions.Logging;
using TTM.Shared.Models.BoersDataRaw.Prices;

namespace boersdata_raw.Domain.Handlers.Sync;

public class SyncDailyPricesHandler(
    ILogger<SyncDailyPricesHandler> logger,
    IBoersDataService boersDataService,
    ISecuritiesRepository securitiesRepository,
    IStockPricesRepository stockPricesRepository,
    IQueueCache<WeeklyRefreshPricesQueue> queue)
    : ISyncSecuritiesDailyPricesHandler
{
    public async Task<List<SecurityPriceDto>> HandleDailyPricesSync()
    {
        logger.LogInformation("Starting to sync securities prices...");

        var securitesTask = securitiesRepository.GetAllSecurities();

        var latestNordicStockpricesTask = boersDataService.GetLatestNordicStockPrices();
        var latestGlobalStockpricesTask = boersDataService.GetLatestGlobalStockPrices();
        
        var securities = await securitesTask;
        var securitiesDiict = securities.ToDictionary(s => s.InsId);
        var latestNordicStockprices = await latestNordicStockpricesTask;
        var latestGlobalStockprices = await latestGlobalStockpricesTask;

        var latestStockPrices = latestGlobalStockprices.Concat(latestNordicStockprices);
        var latestPrices = MakeStockprice(latestStockPrices, securitiesDiict);

        if (!latestPrices.Any())
        {
            logger.LogWarning("No prices were found in latestPrices");
            return new List<SecurityPriceDto>();
        }

        var writeErrors = await stockPricesRepository.SaveBatch(latestPrices);
        logger.LogInformation("{LPCount} stock prices was fetched. {NError} of prices errors",
            latestPrices.Count, writeErrors);

        if (writeErrors != latestPrices.Count)
        {
            // Start the process of there should be a refetch of prices for a security
            queue.Enqueue(new());
        }

        return MakeSecurityPriceDtos(latestPrices);
    }

    private static List<StockPrice> MakeStockprice(IEnumerable<BoersDataLatestStockPrice> latestStockPrices,
        Dictionary<long, Security> securities)
    {
        List<StockPrice> prices = new();
        HashSet<long> addedStockids = new();
        foreach (var nordicStockprice in latestStockPrices.OrderBy(p => p.InstrumentId))
        {
            if (!securities.TryGetValue(nordicStockprice.InstrumentId, out var security))
                continue;

            // To prevent duplicate stocks
            if (!addedStockids.Add(security.InsId))
                continue;

            prices.Add(new StockPrice
            {
                Date = nordicStockprice.Date.ToUniversalTime(),
                Close = nordicStockprice.Close,
                High = nordicStockprice.High,
                Low = nordicStockprice.Low,
                Open = nordicStockprice.Open,
                Volume = nordicStockprice.Volume,
                InsId = nordicStockprice.InstrumentId,
                Ticker = security.Ticker
            });
        }

        return prices;
    }

    private List<SecurityPriceDto> MakeSecurityPriceDtos(List<StockPrice> stockPrices) => stockPrices.Select(s => new
        SecurityPriceDto
        {
            Close = s.Close,
            High = s.High,
            Low = s.Low,
            Open = s.Open,
            Volume = s.Volume,
            Ticker = s.Ticker,
            Date = DateOnly.FromDateTime(s.Date)
        }).ToList();
}