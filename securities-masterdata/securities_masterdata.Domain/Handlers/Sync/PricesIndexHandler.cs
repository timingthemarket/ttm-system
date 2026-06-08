using Microsoft.Extensions.Logging;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Interfaces;
using ttm_system.Shared.Constants;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Handlers.Sync;

public class PricesIndexHandler : IPricesIndexHandler
{
    private readonly ILogger<PricesIndexHandler> _logger;
    private readonly IIndexRepository _indexRepository;
    private readonly ISecurityRepository _securityRepository;

    public PricesIndexHandler(ILogger<PricesIndexHandler> logger, IIndexRepository indexRepository, ISecurityRepository securityRepository)
    {
        _logger = logger;
        _indexRepository = indexRepository;
        _securityRepository = securityRepository;
    }
    
    public async Task HandleDailyPricesIndex()
    {
        var indexesWithSecurities = await _indexRepository.GetIndexWithSecurities();
        var indexvaluesLatest = await _indexRepository.GetLatestIndexValues();
        
        var dateToday = DateOnly.FromDateTime(DateTime.UtcNow);
        _logger.LogInformation("Calculating indexes for {Date}", dateToday);

        var indexValues = new List<IndexValue>();
        foreach (var indexes in indexesWithSecurities)
        {
            var indexSecurityIds = indexes.IndexSecurities.Select(i => i.SecurityId).ToHashSet();
            var pricesByDate = await _securityRepository.GetSecuritiesPricesByDate(dateToday, indexSecurityIds);
            
            // If there are different dates availiable, then we cant calculate the index 
            var distinctDates = pricesByDate.Select(p => p.Date).Distinct().ToList();
            var nrDifferentDates = distinctDates.Count;
            if (nrDifferentDates > 1 || nrDifferentDates < 1)
                continue;

            var latestPriceDate = distinctDates.Single();
            
            // If todays index already exists, then skip calculation
            if (indexvaluesLatest.Any(ivl => indexes.IndexId == ivl.IndexId && ivl.Date == latestPriceDate))
                continue;
            
            // If all of the prices were not returned, then we cant calculate the index 
            if (!pricesByDate.All(pd => indexSecurityIds.Contains(pd.SecurityId)))
                continue;
            
            var securities = indexes.IndexSecurities.ToDictionary(i => i.SecurityId);
            var indexPrice = pricesByDate.Select(p =>
            {
                var security = securities[p.SecurityId];
                return p.Close * security.Weight;
            }).Sum();

            indexValues.Add(new () {IndexId = indexes.IndexId, Value = (decimal)indexPrice, Date = latestPriceDate });
        }

        _logger.LogInformation("Inserting {Count} index values", indexValues.Count);
        await _indexRepository.InsertIndexValues(indexValues);
    }

    /// <summary>
    /// recalculate all index values for a given index
    /// </summary>
    /// <param name="indexId"></param>
    public async Task HandleRecalculateIndexValues(long indexId)
    {
        var indexesWithSecurities = await _indexRepository.GetIndexById(indexId);
        if (indexesWithSecurities == null)
            return;

        _logger.LogInformation("Calculating historical index values for {Ind}", indexesWithSecurities.Name);
        
        var sercurityDict = indexesWithSecurities.IndexSecurities.ToDictionary(i => i.SecurityId);
        var sercurityIds = sercurityDict.Select(s => s.Key).ToHashSet();
        var prices = (await _securityRepository.GetSecuritiesPricesHistory(sercurityIds))
            .GroupBy(p => p.Date)
            .ToDictionary(p => p.Key, p => p.ToList());

        var indexValues = new List<IndexValue>();
        foreach (var priceDate in prices)
        {
            // If all prices does not exist for that date, skip the date
            if (!priceDate.Value.All(pd => sercurityIds.Contains(pd.SecurityId)))
                continue;

            var indexPrice = priceDate.Value.Select(v =>
            {
                var security = sercurityDict[v.SecurityId];
                return v.Close * security.Weight;
            }).Sum();

            indexValues.Add(new() { IndexId = indexId, Value = (decimal)indexPrice, Date = priceDate.Key });
            _logger.LogInformation("Calculated index for {Date}", priceDate.Key);
        }

        _logger.LogInformation("Inserting {Count} historical index values", indexValues.Count);
        await _indexRepository.DeleteIndexValues(indexId);
        await _indexRepository.InsertIndexValues(indexValues);
    }
}