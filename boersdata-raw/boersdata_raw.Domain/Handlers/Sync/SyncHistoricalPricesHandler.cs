using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using boersdata_raw.DataAccess.Models.BoersDataApi;
using boersdata_raw.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using TTM.Shared.Extensions;

namespace boersdata_raw.Domain.Handlers.Sync;

public class SyncHistoricalPricesHandler(
    ILogger<SyncHistoricalPricesHandler> logger,
    IPublishEndpoint publishEndpoint,
    IBoersDataService boersDataService,
    ISecuritiesRepository securitiesRepository,
    IStockPricesRepository stockPricesRepository)
    : ISyncSecuritiesHistoricalPricesHandler
{
    public async Task HandleSelectedSyncHistoricalPrices(List<string> ticker)
    {
        var nordicSecurities = await securitiesRepository.GetNordicSecurities(ticker);
        var globalSecurities = await securitiesRepository.GetGlobalSecurities(ticker);

        var insIds = nordicSecurities.Concat(globalSecurities).Select(s => s.InsId).ToHashSet();
        var historicalPrices = await boersDataService.GetHistoricalStockPrices(insIds);

        await ProcessHistoricalStockPrices(historicalPrices, nordicSecurities.ToDictionary(s => s.InsId));
    }

    public async Task<List<string>> HandleSyncHistoricalPrices()
    {
        var securities = await securitiesRepository.GetAllSecurities();
        var securitiesDict = securities.ToDictionary(s => s.InsId);

        logger.LogInformation("Syncing historical prices for {Count} securities", securities.Count);

        long amountOfRowsSaved = 0;
        var tickerPricesSaved = new List<string>();
        foreach (var securityArry in securities.Chunk(250))
            try
            {
                var instrumentIds = securityArry
                    .Select(sa => sa.InsId)
                    .ToHashSet();

                var taskList = instrumentIds.Chunk(10)
                    .Select(ids => boersDataService.GetHistoricalStockPrices(ids.ToHashSet()))
                    .ToList();

                while (taskList.Any())
                {
                    var historicalPriceTask = await Task.WhenAny(taskList);
                    taskList.Remove(historicalPriceTask);
                    var historicalStockprices = await historicalPriceTask;
                    var savedPrices = await ProcessHistoricalStockPrices(historicalStockprices, securitiesDict);
                    amountOfRowsSaved += savedPrices.Count;
                    tickerPricesSaved.AddRange(savedPrices);
                }
            }
            catch (Exception e)
            {
                await publishEndpoint.SendSystemError(e, nameof(boersdata_raw));
                logger.LogError("Encountered an error {Error} when fetching historical prices", e.Message);
            }

        logger.LogInformation("{NrRows} historical prices have now been saved", amountOfRowsSaved);
        return tickerPricesSaved;
    }

    private async Task<List<string>> ProcessHistoricalStockPrices(
        IReadOnlyList<BoersDataStockPriceArray> historicalStockprices,
        Dictionary<long, Security> securities)
    {
        var pricesSaved = new List<string>();
        foreach (var securityPrices in historicalStockprices)
        {
            if (!securities.TryGetValue(securityPrices.Instrument, out var security))
                continue;

            var prices = securityPrices.StockPricesList.Select(sp => new StockPrice
            {
                Date = sp.Date.Date.ToUniversalTime(),
                Close = sp.Close,
                High = sp.High,
                Low = sp.Low,
                Open = sp.Open,
                Volume = sp.Volume,
                Ticker = security.Ticker,
                InsId = security.InsId
            }).ToList();

            if (!prices.Any()) continue;

            var errors = await stockPricesRepository.OverwriteHistoricalPrices(security.Ticker, prices);
            var nrSaved = prices.Count - errors;
            logger.LogInformation("Saving {Count} historical prices of ticker {Tick}", nrSaved,
                security.Ticker);
            pricesSaved.AddRange(prices.Select(p => p.Ticker));
        }

        return pricesSaved;
    }
}