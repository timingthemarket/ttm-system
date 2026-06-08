using MassTransit;
using Microsoft.Extensions.Logging;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Constants;
using securities_masterdata.Domain.Interfaces;
using ttm_system.Shared.Constants;
using TTM.Shared.Extensions;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Handlers.Sync;

public class DailyPricesHandler(ILogger<DailyPricesHandler> logger, ISecurityRepository securityRepository, ICurrencyRepository currencyRepository, IPublishEndpoint publishEndpoint)
    : IDailyPricesHandler
{
    public async Task<List<SecurityPriceDto>> HandleDailyPrices(List<TTM.Shared.Models.BoersDataRaw.Prices.SecurityPriceDto> securityPrices)
    {
        logger.LogInformation("Received {Count} raw daily prices...", securityPrices.Count);

        var tickers = securityPrices.Where(IsSecurityPriceDtoValid)
            .Select(s => s.Ticker).ToHashSet();
        var securities = await securityRepository.GetSecuritiesByTickers(tickers, true);
        
        var priceDate = securityPrices.Max(s => s.Date);

        var currencyRates = await currencyRepository.GetLatestCurrencyRatesByDate(priceDate);
        
        var secIds = securities.Select(s => s.SecurityId).ToHashSet();
        var insertedSecurities = await securityRepository.GetSecuritiesPricesByDate(priceDate, secIds);
        var savedDatePrices = insertedSecurities.ToDictionary(s => s.SecurityId);

        logger.LogInformation("Got {Count} daily prices that was already inserted...", insertedSecurities.Count);

        var pricesInsert = new List<SecurityPrice>();
        foreach (var security in securities)
        {
            var price = securityPrices.FirstOrDefault(s => s.Ticker == security.Ticker);
            if (price == null)
                continue;

            if (savedDatePrices.TryGetValue(security.SecurityId, out var dailyPrice) && dailyPrice.Date == price.Date)
                continue;

            var currencyRate = currencyRates
                .FirstOrDefault(c => c.CurrencyIdFrom == security.CurrencyId && c.Date == price.Date);
            if (currencyRate == null)
            {
                if (security.Currency.CurrencyCode == FinanceConstants.BaseCurrencyCode)
                {
                    currencyRate = new()
                    {
                        Rate = 1.0,
                        Date = price.Date,
                        CurrencyIdFrom = security.CurrencyId
                    };
                }
                else
                {
                    continue;
                }
            }

            pricesInsert.Add(MapSecurityPriceDto(security.SecurityId, price, currencyRate));
        }

        logger.LogInformation("Adding {Count} daily prices...", pricesInsert.Count);

        await securityRepository.AddSecurityPrices(pricesInsert);
        await publishEndpoint.Increment(MetricNames.DAILY_PRICE_SYNC);
        logger.LogInformation("Saved {Count} daily prices!", pricesInsert.Count);

        return pricesInsert.Select(MapSecurityPriceDto).ToList();
    }

    private static SecurityPrice MapSecurityPriceDto(long securityId,
        TTM.Shared.Models.BoersDataRaw.Prices.SecurityPriceDto sp, CurrencyRate rate) =>
        new()
        {
            SecurityId = securityId,
            Date = sp.Date,
            Volume = sp.Volume!.Value,
            Close = sp.Close!.Value * rate.Rate,
            High = sp.High!.Value * rate.Rate,
            Low = sp.Low!.Value * rate.Rate,
            Open = sp.Open!.Value * rate.Rate
        };

    private static SecurityPriceDto MapSecurityPriceDto(SecurityPrice sp) =>
        new()
        {
            SecurityId = sp.SecurityId,
            Date = sp.Date,
            Volume = sp.Volume,
            Close = sp.Close,
            High = sp.High,
            Low = sp.Low,
            Open = sp.Open
        };

    private bool IsSecurityPriceDtoValid(TTM.Shared.Models.BoersDataRaw.Prices.SecurityPriceDto securityPrice)
    {
        if (!securityPrice.Volume.HasValue)
            return false;

        if (!securityPrice.Close.HasValue)
            return false;

        if (!securityPrice.Open.HasValue)
            return false;

        if (!securityPrice.Low.HasValue)
            return false;

        if (!securityPrice.High.HasValue)
            return false;

        return true;
    }
}