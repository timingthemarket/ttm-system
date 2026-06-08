using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace TTM.Shared.Events.SecuritiesMasterdata;

public class SyncDailyPricesCompleteEvent
{
    public List<SecurityPriceDto> SecurityPrices { get; set; }
}