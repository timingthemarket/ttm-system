using System.Collections.Frozen;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Handlers.Query;

public class QrySecuritiesIndicatorsForPortfolioHandler(
    ILogger<QrySecuritiesIndicatorsForPortfolioHandler> logger,
    IMemoryCache memoryCache,
    IIndicatorsRepository indicatorsRepository,
    IIndicatorsCalculationFactory indicatorsCalculationFactory,
    ISecurityRepository securityRepository)
    : IQrySecuritiesIndicatorsHandler
{
    public async Task<List<SecurityIndicatorDto>> HandleGetIndicators(DateOnly date, List<SecuritiesIndicatorQryMetadataDto> indicators)
    {
        logger.LogInformation("Fetching indicators from date: {Date}", date);
        
        // Not a computed indicator if the lookback is null and !i.IndicatorId.IsComputedIndicator() if the aggregator
        // is Aggregator.Value then it is also just the raw value
        var nonComputedIndicatorIds = indicators
            .Where(i => !i.IndicatorId.IsComputedIndicator() && (i.LookBackPeriod == null || i.LookBackPeriod.Aggregate == Aggregator.Value))
            .Select(i => new { Id = (long)i.IndicatorId, i.LookBackPeriod } ).ToList();

        // Market value limit is 10 000 000 000 SEK
        // TODO: make this into a variable to be sent in
        var securities = await GetFilteredSecurities(1_000_000_000, 20_000, 30);
        //logger.LogInformation("Got {Count} securities from filtering on limit", securities.Count);

        var securityIds = securities.Select(s => s.SecurityId).ToHashSet();
        
        var indicatorsByDate = nonComputedIndicatorIds.Any()
            ? await indicatorsRepository.GetIndicatorsByDate(date, nonComputedIndicatorIds.Select(i => i.Id).ToHashSet(), securityIds)
            : new List<Indicator>();
        
        var indicatorsDtos = MapSecurityIndicatorDtos(indicatorsByDate);
        
        var computedIndicatorIds = indicators
            .Where(i => i.IndicatorId.IsComputedIndicator())
            .ToList();

        string lookBackPeriods = string.Join("|", computedIndicatorIds.Select(c => c.LookBackPeriod?.Period));
        logger.LogInformation("Calculating {Count} computed indicators with LookBackPeriods {Periods}", computedIndicatorIds.Count, lookBackPeriods);
        foreach (var compIndicatorId in computedIndicatorIds)
        {
            var securityIndicators =
                await indicatorsCalculationFactory.Compute(compIndicatorId, securities, date);
            indicatorsDtos.AddRange(securityIndicators);
        }
        
        // Getting the values for indicators to aggregate to Average or Sum
        var aggregateIndicators = indicators.Where(i =>
            !i.IndicatorId.IsComputedIndicator() && i.LookBackPeriod is
                { Aggregate: Aggregator.Average or Aggregator.Sum }).ToList();
        
        foreach (var ind in aggregateIndicators)
        {
            var lookbackDate = date.AddDays(-ind.LookBackPeriod!.Period);
            var aggregatedIndicatorValues = await indicatorsRepository.GetAggregatedIndicatorByDate(lookbackDate, ind.IndicatorId,
                ind.LookBackPeriod!.Aggregate);
            indicatorsDtos.AddRange(MapSecurityIndicatorDtos(aggregatedIndicatorValues));
        }
        
        return indicatorsDtos;
    }
    
    /// <summary>
    /// Get securities that meet the requirements from TODAYs trading volume and value
    /// </summary>
    private async Task<List<Security>> GetFilteredSecurities(decimal marketValueLimit, double volumeLimit, double minimumSekSharePrice)
    {
        var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var cacheKey = $"filteredsecurities-{marketValueLimit}-{volumeLimit}";
        if (memoryCache.TryGetValue(cacheKey, out List<Security>? result) && result is not null && result.Count > 0)
            return result;
        
        var nrSharesSecurities =
            await indicatorsRepository.GetIndicatorsByDate(todayDate,
                new HashSet<long> { (long)Indicators.NumberOfShares });
        var latestPricesByDate = (await securityRepository.GetSecuritiesPricesByDate(todayDate, null))
            .ToDictionary(p => p.SecurityId);
        
        var fromDate = todayDate.AddDays(-30);
        var volumeSecurities = (await securityRepository.GetSecurityIdsByAverageVolume(volumeLimit, fromDate,
            todayDate)).ToHashSet();

        var filteredSecurityIds = new List<long>();
        foreach (var nrSecurityShares in nrSharesSecurities)
        {
            // Skip securities if: //
            
            // 1: Dont qualify for the correct trading volume
            if (!volumeSecurities.Contains(nrSecurityShares.SecurityId))
                continue;
            
            // 2: Has missing price data
            if (!latestPricesByDate.TryGetValue(nrSecurityShares.SecurityId, out var lastPrices))
                continue;
            
            // 3: If the traded price is below minimumSekSharePrice
            if (lastPrices.Close < minimumSekSharePrice)
                continue;
            
            // 4: If the market value is below marketValueLimit
            var actualNrSecurities = nrSecurityShares.Value * 1_000_000;
            if ((decimal)lastPrices.Close * actualNrSecurities <= marketValueLimit)
                continue;
            
            filteredSecurityIds.Add(nrSecurityShares.SecurityId);
        }
        
        var securities = await securityRepository.GetSecuritiesBySecurityIds(filteredSecurityIds.ToHashSet());

        memoryCache.Set(cacheKey, securities, TimeSpan.FromMinutes(15));
        
        return securities;
    }

    private List<SecurityIndicatorDto> MapSecurityIndicatorDtos(List<Indicator> indicators) =>
        indicators.Select(i => new SecurityIndicatorDto
        {
            Value = i.Value,
            IndicatorId = i.IndicatorId,
            SecurityId = i.SecurityId,
            Date = i.Date
        }).ToList();
}