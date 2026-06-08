using MassTransit;
using Microsoft.Extensions.Logging;
using securities_masterdata.DataAccess.Entities;
using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Interfaces;
using ttm_system.Shared.Constants;
using ttm_system.Shared.Events.RiksbankenRaw.Query;
using ttm_system.Shared.Models.RiksbankenRaw;

namespace securities_masterdata.Domain.Handlers.Sync.Backfill;

public class BackfillCurrencyRatesHandler(
    ILogger<BackfillCurrencyRatesHandler> logger,
    IRequestClient<HistoricalCurrenciesQry> client,
    ICurrencyRepository currencyRepository)
    : IBackfillCurrencyRatesHandler
{
    public async Task HandleBackfillCurrencyRates()
    {
        var currencies = await currencyRepository.GetAllCurrencies();
        var toCurrency = currencies.First(c => c.CurrencyCode == FinanceConstants.BaseCurrencyCode);
        foreach (var currency in currencies.Where(c => c.CurrencyCode != FinanceConstants.BaseCurrencyCode))
        {
            var fromCurrency = currencies.First(c => c.CurrencyCode == currency.CurrencyCode);
            var currencyRates =
                await client.GetResponse<CurrencyRateListDto>(new HistoricalCurrenciesQry { Code = currency.CurrencyCode.ToUpper() });

            var dbCurrencyRates = currencyRates.Message.Rates
                .Select(m => MakeCurrencyRates(toCurrency, fromCurrency, m))
                .ToList();

            await currencyRepository.RemoveManyRates(fromCurrency.CurrencyId);
            await currencyRepository.WriteManyRates(dbCurrencyRates);
            logger.LogInformation("Saved {NrCurr1} currency rates. [From Currency: {Curr1}; ToCurrency: {Curr2}] ",
                dbCurrencyRates.Count, currency.CurrencyCode, toCurrency.CurrencyCode);
        }
        
        logger.LogInformation("Backfill done!");
    }

    private CurrencyRate MakeCurrencyRates(Currency toCurr, Currency fromCurr, CurrencyRateDto rate) => new()
    {
        CurrencyIdFrom = fromCurr.CurrencyId,
        CurrencyIdTo = toCurr.CurrencyId,
        Date = DateOnly.FromDateTime(rate.Date),
        Rate = rate.Rate
    };
}