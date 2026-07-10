using MassTransit;
using Microsoft.AspNetCore.Mvc;
using riksbanken_raw.Domain.Interfaces;
using ttm_system.Shared.Events.RiksbankenRaw;

namespace riksbanken_raw.Controllers;

[Route("[controller]")]
[Produces("application/json")]
public class SyncController(IHistoricalCurrencySyncHandler historicalCurrencySyncHandler)
    : ControllerBase
{
    [HttpPost("metadata")]
    public async Task SecuritiesMetadataSync()
    {
        await historicalCurrencySyncHandler.HandleHistoricalCurrencyExchangeSync();
    }
}