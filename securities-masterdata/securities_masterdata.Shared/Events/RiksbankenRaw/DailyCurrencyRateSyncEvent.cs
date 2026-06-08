using ttm_system.Shared.Models.RiksbankenRaw;

namespace ttm_system.Shared.Events.RiksbankenRaw;

public class DailyCurrencyRateSyncEvent
{
    public List<CurrencyRateDto> CurrencyRates { get; set; }
}