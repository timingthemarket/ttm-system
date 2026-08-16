using article_news_raw.Domain.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace article_news_raw.Controllers;

[Route("[controller]")]
[Produces("application/json")]
public class MarketDataController(FetchMarketDataHandler fetchMarketDataHandler) : ControllerBase
{
    /// <summary>
    /// Runs the fetch every registered market data source on demand, bypassing Hangfire/MassTransit.
    /// </summary>
    [HttpGet("trigger-fetch")]
    public async Task<IActionResult> TriggerMarketDataFetch(CancellationToken token)
    {
        await fetchMarketDataHandler.FetchMarketData(token);
        return Ok();
    }
}
