using boersdata_raw.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace boersdata_raw.Controllers;

[Route("[controller]")]
[Produces("application/json")]
public class SyncController(
    ISyncSecuritiesHandler nordicSecuritiesHandler,
    ISyncSecurityMetadataHandler securityMetadataHandler,
    ISyncSecuritiesHistoricalPricesHandler historicalPricesHandler,
    ISyncSecuritiesReportsHandler securityReportsHandler)
    : ControllerBase
{
    /// <summary>
    /// Step 1
    /// </summary>
    /// <returns></returns>
    [HttpPost("metadata")]
    public async Task SecuritiesMetadataSync()
    {
        await securityMetadataHandler.HandleSyncMetadata();
    }

    /// <summary>
    /// Step 2
    /// </summary>
    /// <returns></returns>
    [HttpPost("securities")]
    public async Task SecuritiesSync()
    {
        await nordicSecuritiesHandler.HandleSyncSecurities();
    }

    [HttpPost("historical-prices/{ticker}")]
    public async Task SingleSecuritiesHistroicalPricesSync([FromRoute] string ticker)
    {
        await historicalPricesHandler.HandleSelectedSyncHistoricalPrices([ticker]);
    }

    /// <summary>
    /// This has to be done after Step 1 & 2
    /// </summary>
    [HttpPost("reports-sync")]
    public async Task SecuritiesReportsSync()
    {
        await securityReportsHandler.HandleSyncReports();
    }
    
    /// <summary>
    /// This has to be done after Step 1 & 2
    /// </summary>
    [HttpPost("historical-prices")]
    public async Task SecuritiesHistroicalPricesSync()
    {
        await historicalPricesHandler.HandleSyncHistoricalPrices();
    }
}