using ttm_system.Shared.Events.RiksbankenRaw;

namespace securities_masterdata.Domain.Interfaces;

public interface IDailyCurrencyRatesHandler
{
    Task Handle(DailyCurrencyRateSyncEvent evt);
}