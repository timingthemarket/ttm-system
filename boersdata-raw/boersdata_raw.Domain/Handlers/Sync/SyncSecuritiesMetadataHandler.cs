using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using boersdata_raw.DataAccess.Models.BoersDataApi;
using boersdata_raw.Domain.Enums;
using boersdata_raw.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace boersdata_raw.Domain.Handlers.Sync;

public class SyncSecuritiesMetadataHandler(
    ILogger<SyncSecuritiesMetadataHandler> logger,
    IBoersDataService boersDataService,
    ICountryRepository countryRepository,
    IMarketRepository marketRepository,
    ISectorRepository sectorRepository)
    : ISyncSecurityMetadataHandler
{
    public async Task HandleSyncMetadata()
    {
        logger.LogInformation("Fetching securities metadatas...");

        var countriesTask = boersDataService.GetCountries();
        var industriesTask = boersDataService.GetIndustries();
        var sectorsTask = boersDataService.GetSectors();
        var marketsTask = boersDataService.GetMarkets();
        var translationTask = boersDataService.GetTranslations();

        var translations = MapTranslations(await translationTask);

        var sectorData = MapSectorData(await sectorsTask, await industriesTask, translations);
        var marketData = MapMarketData(await marketsTask, translations);
        var countryData = MapCountryData(await countriesTask, translations);

        var savedMarkets = await marketRepository.SaveBatch(marketData);
        var savedSectors = await sectorRepository.SaveBatch(sectorData);
        var savedCountries = await countryRepository.SaveBatch(countryData);

        logger.LogInformation("Logged {NrMark} markets, {NrSect} sectors and {NrCount} countries", savedMarkets,
            savedSectors,
            savedCountries);
    }

    private Dictionary<TranslationTypes, List<TranslationHandler>> MapTranslations(
        IReadOnlyList<BoersDataTranslationMetadata> translations)
    {
        var countryTrans = new List<TranslationHandler>();
        var sectorTrans = new List<TranslationHandler>();
        var industryTrans = new List<TranslationHandler>();

        foreach (var translation in translations)
        {
            var typeId = GetTranslationType(translation.TranslationKey);
            switch (typeId.Type)
            {
                case TranslationTypes.Country:
                    countryTrans.Add(new TranslationHandler(typeId.Id, translation.NameSv, translation.NameEn));
                    break;
                case TranslationTypes.Industry:
                    industryTrans.Add(new TranslationHandler(typeId.Id, translation.NameSv, translation.NameEn));
                    break;
                case TranslationTypes.Sector:
                    sectorTrans.Add(new TranslationHandler(typeId.Id, translation.NameSv, translation.NameEn));
                    break;
                default:
                    continue;
            }
        }

        return new Dictionary<TranslationTypes, List<TranslationHandler>>
        {
            { TranslationTypes.Country, countryTrans },
            { TranslationTypes.Sector, sectorTrans },
            { TranslationTypes.Industry, industryTrans }
        };
    }

    private (TranslationTypes Type, long Id) GetTranslationType(string translationTranslationKey)
    {
        var splitKey = translationTranslationKey.Split("_");
        if (string.Equals(splitKey[1], "SECTOR", StringComparison.OrdinalIgnoreCase))
            return (TranslationTypes.Sector, long.Parse(splitKey[2]));

        if (string.Equals(splitKey[1], "BRANCH", StringComparison.OrdinalIgnoreCase))
            return (TranslationTypes.Industry, long.Parse(splitKey[2]));

        if (string.Equals(splitKey[1], "COUNTRY", StringComparison.OrdinalIgnoreCase))
            return (TranslationTypes.Country, long.Parse(splitKey[2]));

        return (TranslationTypes.Unknown, -1);
    }

    private List<Country> MapCountryData(IReadOnlyList<BoersDataCountry> countries,
        Dictionary<TranslationTypes, List<TranslationHandler>> translations)
    {
        var coutriesTranslations = translations[TranslationTypes.Country];
        return countries.Select(c =>
        {
            var countryTranslation = coutriesTranslations.FirstOrDefault(ct => ct.Id == c.Id);
            return new Country
            {
                Name = c.Name,
                Translations = new Translations
                {
                    NameEn = countryTranslation?.NameEn, NameSv = countryTranslation?.NameSv
                },
                CountryId = c.Id
            };
        }).ToList();
    }

    private List<Market> MapMarketData(IReadOnlyList<BoersDataMarket> markets,
        Dictionary<TranslationTypes, List<TranslationHandler>> translations)
    {
        var coutriesTranslations = translations[TranslationTypes.Country];
        return markets.Select(m =>
            {
                var countryTranslation = coutriesTranslations.FirstOrDefault(c => c.Id == m.CountryId);
                return new Market
                {
                    Name = $"{countryTranslation?.NameEn} - {m.ExchangeName} - {m.Name}",
                    MarketId = m.Id
                };
            }
        ).ToList();
    }

    private List<Sector> MapSectorData(IReadOnlyList<BoersDataSector> sectors,
        IReadOnlyList<BoersDataIndustry> industries,
        Dictionary<TranslationTypes, List<TranslationHandler>> translations)
    {
        var sectorTranslations = translations[TranslationTypes.Sector];
        var industryTranslations = translations[TranslationTypes.Industry];

        return sectors.Select(s =>
        {
            var industryArry = industries
                .Where(i => i.SectorId == s.Id)
                .Select(i =>
                    {
                        var industryTranslation = industryTranslations.FirstOrDefault(it => it.Id == i.Id);
                        return new Industry
                        {
                            Name = i.Name,
                            Translations = new Translations
                            {
                                NameEn = industryTranslation?.NameEn, NameSv = industryTranslation?.NameSv
                            },
                            IndustryId = i.Id
                        };
                    }
                ).ToList();


            var sectorTranslation = sectorTranslations.FirstOrDefault(it => it.Id == s.Id);
            return new Sector
            {
                Name = s.Name,
                Translations = new Translations
                    { NameEn = sectorTranslation?.NameEn, NameSv = sectorTranslation?.NameSv },
                Industries = industryArry,
                SectorId = s.Id
            };
        }).ToList();
    }

    private sealed record TranslationHandler(long Id, string NameSv, string NameEn);
}