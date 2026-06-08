using TTM.Shared.Models.BoersDataRaw.Prices;

namespace TTM.Shared.Events.BoersDataRaw;

public class RawDailyPricesSyncCompleteEvent
{
    public List<SecurityPriceDto> DailyPrices { get; set; }
}