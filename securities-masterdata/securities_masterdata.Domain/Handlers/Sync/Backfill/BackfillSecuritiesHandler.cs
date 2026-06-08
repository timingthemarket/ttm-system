using MassTransit;
using Microsoft.Extensions.Logging;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.gRPC.Services;
using TTM.Shared.Models.BoersDataRaw;

namespace securities_masterdata.Domain.Handlers.Sync.Backfill;

public class BackfillSecuritiesHandler(
    ILogger<BackfillSecuritiesHandler> logger,
    IBackfillService backfillService,
    ISecurityRepository securityRepository,
    IMarketRepository marketRepository,
    ICurrencyRepository currencyRepository)
    : IBackfillSecuritiesHandler
{
    public async Task HandleBackfillSecurities()
    {
        logger.LogInformation("Starting to backfill securities...");

        var securitiesDto = await backfillService.BackfillSecurities();

        var marketsMap = MapMarkets(securitiesDto);

        var marketsTask = await marketRepository.UpdateAllMarkets(marketsMap);
        var currencyTask = await currencyRepository.GetAllCurrencies();

        var markets = marketsTask.ToDictionary(x => x.Name.ToLower());
        var currencies = currencyTask.ToDictionary(x => x.CurrencyCode.ToLower());

        var dbSecurities = (await securityRepository.GetAllTracked()).ToDictionary(s => s.Ticker);
        
        var save = new List<Security>();
        var updates = new List<Security>();
        foreach (var security in securitiesDto.Securities)
        {
            if (string.IsNullOrEmpty(security.Ticker)) continue;
            if (string.IsNullOrEmpty(security.YahooTicker)) continue;
            if (string.IsNullOrEmpty(security.Currency)) continue;
            if (string.IsNullOrEmpty(security.Name)) continue;
            
            if (dbSecurities.TryGetValue(security.Ticker, out var existingSecurity))
            {
                existingSecurity.YahooTicker = security.YahooTicker;
                existingSecurity.Name = security.Name;
                existingSecurity.Industry = security.Industry?.Name;
                existingSecurity.Updated = DateTime.UtcNow;
                existingSecurity.Sector = security.Sector.Name;
                updates.Add(existingSecurity);
                continue;
            }
            
            if (!markets.TryGetValue(security.Market.Name.ToLower(), out var market))
                continue;
            if (!currencies.TryGetValue(security.Currency.ToLower(), out var currency))
            {
                currency = await currencyRepository.SaveCurrency(new()
                {
                    CurrencyCode = security.Currency,
                    Updated = DateTime.UtcNow,
                    Name = security.Currency
                });
                currencies.Add(security.Currency.ToLower(), currency);
            }

            save.Add(new Security
            {
                Isin = security.Isin,
                Ticker = security.Ticker,
                Name = security.Name,
                Updated = DateTime.UtcNow,
                Country = security.Country.Name,
                Industry = security.Industry?.Name,
                Sector = security.Sector.Name,
                CurrencyId = currency.CurrencyId,
                MarketId = market.MarketId,
                YahooTicker = security.YahooTicker
            });
        }

        var dbSecuritiesTickers = dbSecurities.Select(s => s.Key).ToHashSet();
        var updatesSecurites = updates.Select(s => s.Ticker).ToHashSet();

        var inactiveSecurities = dbSecuritiesTickers.Except(updatesSecurites);
        foreach (var inactiveSecurityTicker in inactiveSecurities)
        {
            var security = dbSecurities[inactiveSecurityTicker];
            security.Updated = DateTime.UtcNow;
            security.Inactive = true;
            updates.Add(security);
        }

        save = SortOutDuplicates(save, currencyTask);
        
        if (save.Any())
        {
            await securityRepository.WriteMany(save);
            logger.LogInformation("Saved {Count} new securities", save.Count);
        }

        int upt = 0;
        if (updates.Any())
        {
            foreach (var update in updates)
            {
                await securityRepository.Update(update);
                upt++;
                if (upt % 100 == 0)
                    logger.LogInformation("Updated {Count}/{Count2} securities", upt, updates.Count);
            }
            logger.LogInformation("Updated {Count}/{Count2} securities", upt, updates.Count);
        }
        
        logger.LogInformation("Backfill of securities done!");
    }

    /// <summary>
    /// Can be duplicate tickernames
    /// </summary>
    private List<Security> SortOutDuplicates(List<Security> securities, List<Currency> currencies)
    {
        var currencyDict = currencies.ToDictionary(x => x.CurrencyId);
        
        var cleanedDuplicateSecurities = securities.GroupBy(s => s.Ticker)
            .Where(g => g.Count() > 1)
            .Select(g => g.OrderBy(s => MapCurrecyImportance(currencyDict[s.CurrencyId].CurrencyCode)).First())
            .ToList();

        var nonDuplicateSecurites = securities.GroupBy(s => s.Ticker)
            .Where(g => g.Count() == 1)
            .Select(g => g.First())
            .ToList();
        
        return nonDuplicateSecurites.Concat(cleanedDuplicateSecurities).ToList();
    }

    private static int MapCurrecyImportance(string currencyCode) => 
        currencyCode switch
    {
        "SEK" => 1,
        "DKK" => 2,
        "NOK" => 3,
        "USD" => 4,
        "EUR" => 5,
        "GBP" => 6,
        _ => 7
    };

    private List<Market> MapMarkets(SecuritiesQryResponse securitiesQryResponseDto) => securitiesQryResponseDto
        .Securities.DistinctBy(s => s.Market.Name).Select(s => new Market
        {
            Name = s.Market.Name,
            Updated = DateTime.UtcNow
        }).ToList();
}