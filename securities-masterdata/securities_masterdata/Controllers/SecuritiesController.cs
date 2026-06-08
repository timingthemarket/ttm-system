using MassTransit;
using Microsoft.AspNetCore.Mvc;
using securities_masterdata.Domain.Constants;
using securities_masterdata.Domain.Interfaces;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;
using TTM.Shared.Models.SecuritiesMasterdata;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Controllers;

[Route("[controller]")]
[Produces("application/json")]
public class SecuritiesController(
    IQrySecuritiesPricesHandler qrySecuritiesPrices,
    IQrySecuritiesIndicatorsHandler qrySecuritiesIndicatorsHandler,
    IQrySecuritiesHandler qrySecuritiesHandler)
    : ControllerBase
{
    [HttpGet("date/price")]
    public async Task<ActionResult<List<SecurityPriceDto>>> SecuritiesPrices([FromQuery] string date,
        [FromQuery] string securityIds)
    {
        var parseDate = DateOnly.Parse(date);
        var tickersSplit = securityIds.Split(",").Select(long.Parse).ToHashSet();

        var prices = await qrySecuritiesPrices.HandleGetTickerDatePrices(parseDate, tickersSplit);
        return prices;
    }
    
    [HttpPost("indicators")]
    public async Task<ActionResult<List<SecurityIndicatorDto>>> SecuritiesIndicators([FromBody] SecuritiesIndicatorsQry qry)
    {
        return await qrySecuritiesIndicatorsHandler.HandleGetIndicators(qry.Date, qry.Indicators);
    }

    [HttpGet("supported_indicators")]
    public List<Indicators> GetSupportedCalculationIndicators() => SupportedCalculationIndicators.SupportedIndicators;
    
    [HttpPost("securities")]
    public async Task<List<SecurityDto>> GetSecurities([FromBody] SecuritiesQry qry) => await qrySecuritiesHandler.HandleGetSecurities(qry);
}