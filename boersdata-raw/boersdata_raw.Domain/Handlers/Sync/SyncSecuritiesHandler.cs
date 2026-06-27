using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using boersdata_raw.DataAccess.Models.BoersDataApi;
using boersdata_raw.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace boersdata_raw.Domain.Handlers.Sync;

public class SyncSecuritiesHandler(
    ILogger<SyncSecuritiesHandler> logger,
    IBoersDataService boersDataService,
    ISecuritiesRepository securitiesRepository)
    : ISyncSecuritiesHandler
{
    public async Task HandleSyncSecurities()
    {
        logger.LogInformation("Starting to sync nordic securities...");

        var nordicInstruments = await boersDataService.GetNordicInstruments();
        var globalInstruments = await boersDataService.GetGlobalInstruments();

        logger.LogInformation("Fetched {NordicInstrumentsCount} nordic instruments and {GlobalInstrumentsCount}",
            nordicInstruments.Count,
            globalInstruments.Count);
        var nordicSecurities = MapToSecurities(nordicInstruments);

        long updated = 0;
        await securitiesRepository.DeleteAllNordic();
        foreach (var securityChunk in nordicSecurities.Chunk(100))
        {
            updated += await securitiesRepository.SaveBatch(securityChunk.ToList());
            logger.LogInformation("Saved batch of securities [{Progress}/{Total}]",
                updated, nordicInstruments.Count);
        }

        var globalSecurities = MapToSecurities(globalInstruments).ToList();
        globalSecurities = FilterAwayDuplicateSecurites(globalSecurities);

        await securitiesRepository.DeleteAllGlobal();
        foreach (var securityChunk in globalSecurities.Chunk(100))
        {
            updated += await securitiesRepository.SaveGlobalBatch(securityChunk.ToList());
            logger.LogInformation("Saved batch of securities [{Progress}/{Total}]",
                updated, globalInstruments.Count);
        }

        logger.LogInformation("Securities sync is done. Updated {Updated} securities", updated);
    }


    /// <summary>
    /// Perform some securities mapping and some preliminary filtering of securities we are not interested in
    /// </summary>
    /// <param name="instruments"></param>
    /// <returns></returns>
    private IEnumerable<Security> MapToSecurities(IReadOnlyList<BoersDataInstrument> instruments) => instruments
        .Select(ins =>
            new Security
            {
                Ticker = ins.Ticker,
                Name = ins.Name,
                Isin = ins.Isin,
                Type = GetSecurityType(ins),
                CountryId = ins.CountryId,
                IndustryId = ins.BranchId,
                MarketId = ins.MarketId,
                SectorId = ins.SectorId,
                InsId = ins.InsId,
                UrlName = ins.UrlName,
                YahooTicker = ins.Yahoo,
                Currency = ins.StockPriceCurrency,
                ReportCurrency = ins.ReportCurrency
            })
        .Where(s => new List<SecurityType> { SecurityType.Adr, SecurityType.Stocks }.Contains(s.Type));

    private static List<Security> FilterAwayDuplicateSecurites(List<Security> securities)
    {
        // ISSUE: There are duplicate Tickers, so for now just ignore those duplicates
        // In the furitre we might want to have the ISIN as the primary indentification
        
        var nonDuplicateTickers = securities.GroupBy(s => s.Ticker)
            .Where(g => g.Count() == 1)
            .Select(g => g.First())
            .ToList();
        
        // Only do a psudo random filtering of duplicate tickers based on the country ID
        var filteredDuplicates = securities.GroupBy(s => s.Ticker)
            .Where(g => g.Count() > 1)
            .Select(g => g.OrderBy(s => s.CountryId).First())
            .ToList();

        return nonDuplicateTickers.Concat(filteredDuplicates).ToList();
    }
    
    private SecurityType GetSecurityType(BoersDataInstrument ins)
    {
        switch (ins.Instrument)
        {
            case InstrumentType.Pref:
                return SecurityType.Pref;
            case InstrumentType.Currency:
                return SecurityType.Currency;
            case InstrumentType.Commodity:
                return SecurityType.Commodity;
            case InstrumentType.Spac:
                return SecurityType.Spac;
            case InstrumentType.Adr:
                return SecurityType.Adr;
            case InstrumentType.Unit:
                return SecurityType.Unit;
            case InstrumentType.Cryptocurrencies:
                return SecurityType.Cryptocurrencies;
            case InstrumentType.SectorIndex:
            case InstrumentType.IndustryIndex:
            case InstrumentType.Index:
            case InstrumentType.GlobalIndex:
            case InstrumentType.OtherIndex:
                return SecurityType.Index;
            default:
                return SecurityType.Stocks;
        }
    }
}