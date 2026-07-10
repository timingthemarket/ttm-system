using MassTransit;
using riksbanken_raw.Domain.Events;
using riksbanken_raw.Domain.Interfaces;
using ttm_system.Shared.Events.RiksbankenRaw;

namespace riksbanken_raw.Triggers;

public class SyncDailyCurrencyRatesTrigger : IConsumer<SyncDailyCurrencyTriggerEvent>
{
    private readonly ILogger<SyncDailyCurrencyRatesTrigger> _logger;
    private readonly ICurrencySyncHandler _currencySyncHandler;

    public SyncDailyCurrencyRatesTrigger(ILogger<SyncDailyCurrencyRatesTrigger> logger, ICurrencySyncHandler currencySyncHandler)
    {
        _logger = logger;
        _currencySyncHandler = currencySyncHandler;
    }

    public async Task Consume(ConsumeContext<SyncDailyCurrencyTriggerEvent> context)
    {
        _logger.LogInformation("Running currency sync");

        var latestCurrencies = await _currencySyncHandler.HandleLatestCurrencyExchangeSync();
        
        if (latestCurrencies.Any())
            await context.Publish(new DailyCurrencyRateSyncEvent { CurrencyRates = latestCurrencies });
    }
}