using MassTransit;
using securities_masterdata.Domain.Interfaces;
using ttm_system.Shared.Events.RiksbankenRaw;

namespace securities_masterdata.Consumers.RiksbankenRaw;

public class SyncDailyCurrencyRatesConsumer : IConsumer<DailyCurrencyRateSyncEvent>
{
    private readonly IDailyCurrencyRatesHandler _ratesHandler;

    public SyncDailyCurrencyRatesConsumer(IDailyCurrencyRatesHandler ratesHandler)
    {
        _ratesHandler = ratesHandler;
    }

    public async Task Consume(ConsumeContext<DailyCurrencyRateSyncEvent> context)
    {
        await _ratesHandler.Handle(context.Message);
    }
}