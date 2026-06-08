using Microsoft.Extensions.Logging;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Interfaces;
using ttm_system.Shared.Events.RiksbankenRaw;
using ttm_system.Shared.Models.RiksbankenRaw;

namespace securities_masterdata.Domain.Handlers.Sync;

public class DailyCurrencyRatesHandler : IDailyCurrencyRatesHandler
{
    private readonly ILogger<DailyCurrencyRatesHandler> _logger;
    private readonly ICurrencyRepository _currencyRepository;

    public DailyCurrencyRatesHandler(ILogger<DailyCurrencyRatesHandler> logger, ICurrencyRepository currencyRepository)
    {
        _logger = logger;
        _currencyRepository = currencyRepository;
    }
    public async Task Handle(DailyCurrencyRateSyncEvent evt)
    {
        _logger.LogInformation("Syncing {Count} amount of currency rates", evt.CurrencyRates.Count);
        
        var currencies = await _currencyRepository.GetAllCurrencies();
        foreach (var rate in evt.CurrencyRates)
        {
            var fromCurrency = currencies.FirstOrDefault(c => c.CurrencyCode == rate.FromCode);
            var toCurrency = currencies.FirstOrDefault(c => c.CurrencyCode == rate.ToCode);
            if (fromCurrency == null || toCurrency == null)
                continue;

            var dbRate = MakeCurrencyRates(toCurrency, fromCurrency, rate);
            await _currencyRepository.SaveRate(dbRate);
        }
    }

    private CurrencyRate MakeCurrencyRates(Currency toCurr, Currency fromCurr, CurrencyRateDto rate) => new()
    {
        CurrencyIdFrom = fromCurr.CurrencyId,
        CurrencyIdTo = toCurr.CurrencyId,
        Date = DateOnly.FromDateTime(rate.Date),
        Rate = rate.Rate
    };
}