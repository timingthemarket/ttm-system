using Microsoft.AspNetCore.Mvc;
using securities_masterdata.Domain.Interfaces;

namespace securities_masterdata.Controllers;

[Route("[controller]")]
[Produces("application/json")]
public class BackfillController : ControllerBase
{
    private readonly IBackfillCurrencyRatesHandler _backfillCurrencyRatesHandler;
    private readonly IBackfillReportsHandler _backfillReportsHandler;
    private readonly IPricesIndexHandler _pricesIndexHandler;
    private readonly IBackfillSecuritiesHandler _backfillSecuritiesHandler;
    private readonly IBackfillSecuritiesPricesHandler _backfillSecuritiesPricesHandler;

    public BackfillController(IBackfillCurrencyRatesHandler backfillCurrencyRatesHandler,
        IBackfillSecuritiesHandler backfillSecuritiesHandler,
        IBackfillSecuritiesPricesHandler backfillSecuritiesPricesHandler,
        IBackfillReportsHandler backfillReportsHandler,
        IPricesIndexHandler pricesIndexHandler)
    {
        _backfillCurrencyRatesHandler = backfillCurrencyRatesHandler;
        _backfillSecuritiesHandler = backfillSecuritiesHandler;
        _backfillSecuritiesPricesHandler = backfillSecuritiesPricesHandler;
        _backfillReportsHandler = backfillReportsHandler;
        _pricesIndexHandler = pricesIndexHandler;
    }

    [HttpPost("currency-rates")]
    public async Task<IActionResult> BackfillCurrencyRates()
    {
        await _backfillCurrencyRatesHandler.HandleBackfillCurrencyRates();
        return Ok("success");
    }

    [HttpPost("securities")]
    public async Task<IActionResult> BackfillSecurities()
    {
        await _backfillSecuritiesHandler.HandleBackfillSecurities();
        return Ok("success");
    }

    [HttpPost("securities-prices")]
    public async Task<IActionResult> BackfillSecuritiesPrices()
    {
        await _backfillSecuritiesPricesHandler.HandleBackfillSecuritiesPrices();
        return Ok("success");
    }

    [HttpPost("securities-prices/{ticker}")]
    public async Task<IActionResult> BackfillSecuritiesPrices(string ticker)
    {
        await _backfillSecuritiesPricesHandler.HandleBackfillSecurityPrices(new () { ticker });
        return Ok("success");
    }

    [HttpPost("reports")]
    public async Task<IActionResult> BackfillReports()
    {
        await _backfillReportsHandler.HandleBackfillReports();
        return Ok("success");
    }

    [HttpPost("reports/ticker")]
    public async Task<IActionResult> BackfillReportsTickers([FromBody] List<string> tickers)
    {
        await _backfillReportsHandler.HandleBackfillReports(tickers);
        return Ok("success");
    }


    [HttpPost("index/{indexId:int}")]
    public async Task<IActionResult> BackfillIndex(int indexId)
    {
        await _pricesIndexHandler.HandleRecalculateIndexValues(indexId);
        return Ok("success");
    }
}