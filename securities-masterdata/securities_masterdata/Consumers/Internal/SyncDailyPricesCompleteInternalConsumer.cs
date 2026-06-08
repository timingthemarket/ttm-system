using MassTransit;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.Events.SecuritiesMasterdata;

namespace securities_masterdata.Consumers.Internal;

public class SyncDailyPricesCompleteInternalConsumer : IConsumer<SyncDailyPricesCompleteEvent>
{
    private readonly IPricesIndexHandler _pricesIndexHandler;

    public SyncDailyPricesCompleteInternalConsumer(IPricesIndexHandler pricesIndexHandler) =>
        _pricesIndexHandler = pricesIndexHandler;

    public async Task Consume(ConsumeContext<SyncDailyPricesCompleteEvent> context)
    {
        await _pricesIndexHandler.HandleDailyPricesIndex();
    }
}