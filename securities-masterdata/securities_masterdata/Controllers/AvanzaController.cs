using Microsoft.AspNetCore.Mvc;
using securities_masterdata.Domain.Interfaces;
using securities_masterdata.Domain.Models.Sync;

namespace securities_masterdata.Controllers;

[Route("[controller]")]
[Produces("application/json")]
public class AvanzaController : ControllerBase
{
    private readonly IAvanzaSyncHandler _avanzaSyncHandler;

    public AvanzaController(IAvanzaSyncHandler avanzaSyncHandler)
    {
        _avanzaSyncHandler = avanzaSyncHandler;
    }

    [HttpPost("sync-securities")]
    public async Task<ActionResult<AvanzaSyncResult>> SyncSecurities(CancellationToken cancellationToken = default)
    {
        var result = await _avanzaSyncHandler.HandleSyncSecuritiesWithAvanza(cancellationToken);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }
}