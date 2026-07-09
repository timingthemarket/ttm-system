using boersdata_raw.DataAccess.Interfaces;
using boersdata_raw.DataAccess.Models;
using boersdata_raw.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using TTM.Shared.Models.BoersDataRaw.Securities;

namespace boersdata_raw.Domain.Handlers.Query;

public class QrySecuritiesHandler(
    ILogger<QrySecuritiesHandler> logger,
    ICountryRepository countryRepository,
    IMarketRepository marketRepository,
    ISectorRepository sectorRepository,
    ISecuritiesRepository securitiesRepository)
    : IQrySecuritiesHandler
{
    private readonly ILogger<QrySecuritiesHandler> _logger = logger;

    public async Task<List<SecurityDto>> HandleGetSecurities()
    {
        var securitiesTask = securitiesRepository.GetAllSecurities();
        var sectorsTask = sectorRepository.GetAll();
        var marketsTask = marketRepository.GetAll();
        var countryTask = countryRepository.GetAll();

        var securites = await securitiesTask;
        var sectors = (await sectorsTask).ToDictionary(s => s.SectorId);
        var markets = (await marketsTask).ToDictionary(m => m.MarketId);
        var countries = (await countryTask).ToDictionary(c => c.CountryId);

        var returnList = new List<SecurityDto>();
        foreach (var security in securites)
        {
            if (!security.SectorId.HasValue)
                continue;
            if (!sectors.TryGetValue(security.SectorId.Value, out var sector))
                continue;
            if (!markets.TryGetValue(security.MarketId, out var market))
                continue;
            if (!countries.TryGetValue(security.CountryId, out var country))
                continue;
            if (string.IsNullOrEmpty(security.Currency))
                continue;

            var dto = MakeSecurityDto(security, market, sector, country, security.Currency);
            returnList.Add(dto);
        }

        return returnList;
    }

    private SecurityDto MakeSecurityDto(Security security, Market market, Sector sector, Country country,
        string currency)
    {
        var industry = sector.Industries.FirstOrDefault(i => i.IndustryId == security.IndustryId);

        return new SecurityDto
        {
            Ticker = security.Ticker,
            Name = security.Name,
            Isin = security.Isin,
            Type = security.Type.ToString(),
            Currency = currency,
            Country = new CountryDto
            {
                Name = country.Translations.NameEn ?? country.Name
            },
            Market = new MarketDto { Name = market.Name },
            Sector = new SectorDto
            {
                Name = sector.Translations.NameEn ?? sector.Name
            },
            Industry = industry is null
                ? null
                : new IndustryDto { Name = industry.Translations.NameEn ?? industry.Name },
            YahooTicker = security.YahooTicker
        };
    }
}