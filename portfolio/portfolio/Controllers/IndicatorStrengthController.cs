using Microsoft.AspNetCore.Mvc;
using portfolio.Domain.Interfaces;

namespace portfolio.Controllers;

[Route("[controller]")]
[Produces("application/json")]
public class IndicatorStrengthController(IIndicatorStrengthHandler indicatorStrengthHandler) : ControllerBase
{
    /// <summary>
    /// Runs the indicator strength backtest over the monthly rebalance grid ending at
    /// <paramref name="date"/>. The backfill is long running - it fetches prices and indicator
    /// values per rebalance date - so expect the request to stay open for several minutes.
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromQuery] DateOnly? date, [FromQuery] int backfillYears = 12,
        CancellationToken cancellationToken = default)
    {
        if (backfillYears < 1)
        {
            return BadRequest("Backfill years must be at least 1.");
        }

        DateOnly today = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        await indicatorStrengthHandler.ProcessIndicatorStrength(today, backfillYears, cancellationToken);

        return Ok();
    }
}