using Microsoft.AspNetCore.Mvc;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Models;

namespace portfolio.Controllers;

[Route("[controller]")]
[Produces("application/json")]
public class PortfolioController(
    IComputePortfolioHandler computePortfolioHandler,
    IYahooExportService yahooExportService,
    IPortfolioPerformanceHandler portfolioPerformanceHandler) : ControllerBase
{
    [HttpPost("compute")]
    public async Task<SecuritiesPortfolioQryResponse> ComputePortfolio([FromBody] SecuritiesPortfolioQry qry)
    {
        var portfolio = await computePortfolioHandler.HandleComputePortfolio(qry.Date, qry.StrategyId, qry.RowSimilarityLimit, 
            qry.Variables, qry.Money, qry.MaxSecuritySpending);
        return portfolio;
    }
    
    [HttpPost("export/yahoo")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportYahooCsv([FromBody] YahooExportQry qry)
    {
        var file = await yahooExportService.ExportYahooDataToFile(qry.Money, qry.PortfolioId);
        var dt = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var fileName = $"yahoo_portfolio_{dt}_{qry.PortfolioId}.csv";
        return File(file, "text/csv", fileName);
    }

    [HttpPost("export/yahoo/by-set")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportYahooCsvBySetId([FromBody] YahooExportBySetIdQry qry)
    {
        var file = await yahooExportService.ExportYahooDataToFileBySetId(qry.Money, qry.SetId);
        var dt = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var fileName = $"yahoo_portfolio_{dt}_{qry.SetId}.csv";
        return File(file, "text/csv", fileName);
    }

    [HttpPost("performance")]
    public async Task<ActionResult<PortfolioPerformanceResponse>> GetPerformance([FromBody] PortfolioPerformanceQry qry)
    {
        var result = await portfolioPerformanceHandler.GetPerformanceBySetId(qry.SetId, qry.Date);
        if (result == null)
        {
            return NotFound();
        }
        return result;
    }
}