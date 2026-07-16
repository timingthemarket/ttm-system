using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.Constants;
using TTM.Shared.Models.SecuritiesMasterdata;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Handlers.Query;

public class QrySecuritiesHandler(
    ILogger<QrySecuritiesHandler> logger,
    ISecurityRepository securityRepository,
    ICurrencyRepository currencyRepository)
    : IQrySecuritiesHandler
{
    public async Task<List<SecurityDto>> HandleGetSecurities(SecuritiesQry qry)
    {
        // TODO: make filtering of market value and trading volume here
        List<Security>? securities = null;
        if (qry.SecurityIds != null && qry.SecurityIds.Any())
        {
            securities = await securityRepository.GetSecuritiesBySecurityIds(qry.SecurityIds.ToHashSet(), true);
        }
        
        if (qry.Tickers != null && qry.Tickers.Any())
        {
            securities = await securityRepository.GetSecuritiesByTickers(qry.Tickers.ToHashSet());
        }

        bool isSecurityEmpty = securities == null || !securities.Any();
        securities ??= await securityRepository.GetAll();
        
        var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var securityIds = isSecurityEmpty ? null : securities.Select(s => s.SecurityId).ToHashSet();
        var latestPricesByDate = await securityRepository.GetSecuritiesPricesByDate(todayDate, securityIds);

        // Fetch currency rates if conversion is requested
        Dictionary<long, CurrencyRate>? currencyRatesByCurrencyId = null;
        if (qry.ConvertPricesToOriginal)
        {
            var currencyRates = await currencyRepository.GetLatestCurrencyRatesByDate(todayDate);
            currencyRatesByCurrencyId = currencyRates.ToDictionary(cr => cr.CurrencyIdFrom);
            logger.LogInformation("Fetched {Count} currency rates for conversion", currencyRates.Count);
        }

        logger.LogInformation("Fetched {Count} securities", securities.Count);

        return MapSecurityDtos(
            securities,
            latestPricesByDate.ToFrozenDictionary(p => p.SecurityId),
            qry.ConvertPricesToOriginal,
            currencyRatesByCurrencyId);
    }

    // Only return securities that have prices
    private List<SecurityDto> MapSecurityDtos(
        List<Security> securities,
        FrozenDictionary<long, SecurityPrice> securityPrices,
        bool convertPricesToOriginal,
        Dictionary<long, CurrencyRate>? currencyRatesByCurrencyId) => securities
        .Where(s => securityPrices.ContainsKey(s.SecurityId))
        .Select(s =>
        {
            var price = securityPrices[s.SecurityId];
            var convertedPrice = convertPricesToOriginal
                ? ConvertPriceToOriginalCurrency(price.Close, s.CurrencyId, s.Currency.CurrencyCode, currencyRatesByCurrencyId)
                : price.Close;

            return new SecurityDto
            {
                SecurityId = s.SecurityId,
                Name = s.Name,
                Isin = s.Isin,
                Ticker = s.Ticker,
                Country = s.Country,
                Description = s.Description,
                Industry = s.Industry,
                Sector = s.Sector,
                Updated = s.Updated,
                Market = s.Market.Name,
                MarketId = s.MarketId,
                CurrencyId = s.CurrencyId,
                CurrencyCode = s.Currency.CurrencyCode,
                YahooTicker = s.YahooTicker ?? "",
                LatestRawPrice = convertedPrice
            };
        }).ToList();

    /// <summary>
    /// Converts a price from SEK (storage currency) to the original security currency.
    /// Formula: originalPrice = sekPrice / rate.Rate (inverse of storage conversion)
    /// </summary>
    private double ConvertPriceToOriginalCurrency(
        double sekPrice,
        long currencyId,
        string currencyCode,
        Dictionary<long, CurrencyRate>? currencyRatesByCurrencyId)
    {
        // If currency is already SEK, no conversion needed
        if (currencyCode == FinanceConstants.BaseCurrencyCode)
        {
            return sekPrice;
        }

        // If no rates dictionary provided or rate not found, return SEK price with warning
        if (currencyRatesByCurrencyId == null || !currencyRatesByCurrencyId.TryGetValue(currencyId, out var rate))
        {
            logger.LogWarning(
                "Currency rate not found for currency {CurrencyCode} (ID: {CurrencyId}), returning price in SEK",
                currencyCode,
                currencyId);
            return sekPrice;
        }

        // Convert back to original currency: originalPrice = sekPrice / rate.Rate
        return sekPrice / rate.Rate;
    }
}