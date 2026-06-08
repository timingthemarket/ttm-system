using Microsoft.AspNetCore.Mvc;
using securities_masterdata.Domain.Interfaces;
using securities_masterdata.Domain.Models.Command;

namespace securities_masterdata.Controllers;

[Route("[controller]")]
[Produces("application/json")]
public class IndexController : ControllerBase
{
    private readonly ICmdAddIndexSecurityHandler _indexSecurityHandler;

    public IndexController(ICmdAddIndexSecurityHandler indexSecurityHandler)
    {
        _indexSecurityHandler = indexSecurityHandler;
    }

    [HttpPost("security")]
    public async Task<IActionResult> AddIndexSecurity([FromBody] AddIndexSecurityCmd cmd)
    {
        await _indexSecurityHandler.HandleAddIndexSecurity(cmd);
        return Ok("success");
    }
}