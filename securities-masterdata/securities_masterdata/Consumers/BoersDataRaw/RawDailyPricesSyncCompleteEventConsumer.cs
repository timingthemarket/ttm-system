using MassTransit;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.Events.BoersDataRaw;
using TTM.Shared.Events.SecuritiesMasterdata;

namespace securities_masterdata.Consumers.BoersDataRaw;

public class RawDailyPricesSyncCompleteEventConsumer : IConsumer<RawDailyPricesSyncCompleteEvent>
{
    private readonly IDailyPricesHandler _dailyPricesHandler;

    public RawDailyPricesSyncCompleteEventConsumer(ILogger<RawDailyPricesSyncCompleteEventConsumer> logger,
        IDailyPricesHandler dailyPricesHandler) =>
        _dailyPricesHandler = dailyPricesHandler;

    public async Task Consume(ConsumeContext<RawDailyPricesSyncCompleteEvent> context)
    {
        var prices =  await _dailyPricesHandler.HandleDailyPrices(context.Message.DailyPrices);
        await context.Publish(new SyncDailyPricesCompleteEvent { SecurityPrices = prices });
    }
}