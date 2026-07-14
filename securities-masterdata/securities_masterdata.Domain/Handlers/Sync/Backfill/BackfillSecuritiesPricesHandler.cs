using MassTransit;
using Microsoft.Extensions.Logging;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Constants;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;
using TTM.Shared.gRPC.Services;
using TTM.Shared.Models.BoersDataRaw;
using TTM.Shared.Models.BoersDataRaw.Prices;

namespace securities_masterdata.Domain.Handlers.Sync.Backfill;

public class BackfillSecuritiesPricesHandler(
    ILogger<BackfillSecuritiesPricesHandler> logger,
    IBackfillService backfillService,
    ISecurityRepository securityRepository,
    ICurrencyRepository currencyRepository,
    IPublishEndpoint publishEndpoint)
    : IBackfillSecuritiesPricesHandler
{
    public async Task HandleBackfillSecurityPrices(List<string> tickers)
    {
        tickers = tickers.Select(t => t.ToUpper()).ToList();
        
        logger.LogInformation("Starting to backfill securities prices for {Ticker}", string.Join("|", tickers));

        var securitiesFull = await securityRepository.GetSecuritiesByTickers(tickers.ToHashSet(), true);
        
        await GetAllSecuritiesPrices(securitiesFull);
    }
    
    private async IAsyncEnumerable<HistoricalPricesQry> GetHistoricalPricesQry(List<string> tickers)
    {
        foreach (var chunk in tickers.Chunk(10))
        {
            var ret = await Task.FromResult(new HistoricalPricesQry
            {
                Tickers = chunk.ToList()
            });
            yield return ret;
        }
    }
    
    public async Task HandleBackfillSecuritiesPrices()
    {
        logger.LogInformation("Starting to backfill ALL securities prices...");
        var securitiesFull = await securityRepository.GetAll();
        
        await GetAllSecuritiesPrices(securitiesFull);
    }
    
    private async Task GetAllSecuritiesPrices(List<Security> securities)
    {
        var rates = await currencyRepository.GetAllCurrencyRates();
        var ratesDict = rates.GroupBy(r => r.CurrencyIdFrom)
            .ToDictionary(r => r.Key, r => r.ToList());
        
        var tickers = securities.Select(s => s.Ticker).ToList();
        var requestStream = GetHistoricalPricesQry(tickers);

        var securitiesDict = securities.ToDictionary(s => s.Ticker);

        var tickersFetched = new List<string>();
        await foreach (var historicalPrice in backfillService.BackfillHistoricalPrices(requestStream))
        {
            var security = securitiesDict[historicalPrice.Ticker];

            if (!ratesDict.TryGetValue(security.CurrencyId, out var securityRates))
            {
                // Swedish rates wont exist in the DB
                if (security.Currency.CurrencyCode == FinanceConstants.BaseCurrencyCode)
                {
                    securityRates = new()
                    {
                        new()
                        {
                            Rate = 1,
                            Date = new ()
                        }
                    };
                }
                else
                {
                    continue;
                }
            }
            
            var securityPrices = MapSecurityPriceDto(security.SecurityId, historicalPrice.HistoricalPrices,
                securityRates);

            await securityRepository.UpdateAndReplaceAllSecurityPrices(security.SecurityId, securityPrices, true);
            await publishEndpoint.Increment(MetricNames.BACKFILL_SECURITIY_PRICES_CHUNK);

            tickersFetched.Add(security.Ticker);
            if (tickersFetched.Count >= 10)
            {
                logger.LogInformation("Prices fetched for {Tickers}", tickersFetched);
                tickersFetched.Clear();
            }
        }

        if (tickersFetched.Any())
            logger.LogInformation("Prices fetched for {Tickers}", tickersFetched);
    }

    private List<SecurityPrice> MapSecurityPriceDto(long securityId, List<SecurityPriceDto> securityPrice,
        List<CurrencyRate> rates)
    {
        rates = rates.OrderByDescending(r => r.Date).ToList();
        
        return securityPrice
            .Where(IsSecurityPriceDtoValid)
            .Select(s =>
            {
                var sRate = rates.First(rate => rate.Date <= s.Date);
                
                return new SecurityPrice
                {
                    SecurityId = securityId,
                    Date = s.Date,
                    Volume = s.Volume!.Value,
                    Close = s.Close!.Value * sRate.Rate,
                    High = s.High!.Value * sRate.Rate,
                    Low = s.Low!.Value * sRate.Rate,
                    Open = s.Open!.Value * sRate.Rate
                };
            }).ToList();
    }


    private bool IsSecurityPriceDtoValid(SecurityPriceDto securityPrice)
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