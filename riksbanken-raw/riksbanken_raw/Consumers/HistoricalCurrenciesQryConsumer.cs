using MassTransit;
using riksbanken_raw.Domain.Interfaces;
using ttm_system.Shared.Events.RiksbankenRaw.Query;
using ttm_system.Shared.Models.RiksbankenRaw;

namespace riksbanken_raw.Consumers;

public class HistoricalCurrenciesQryConsumer(
    ICurrencyQryHandler currencyQryHandler)
    : IConsumer<HistoricalCurrenciesQry>
{
    public async Task Consume(ConsumeContext<HistoricalCurrenciesQry> context)
    {
        var currencies = await currencyQryHandler.GetHistoricalCurrenciesByCode(context.Message.Code);
        await context.RespondAsync(new CurrencyRateListDto {Rates = currencies});
    }
}