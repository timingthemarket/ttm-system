using Microsoft.AspNetCore.Mvc;
using portfolio.DataAccess.Interfaces;
using portfolio.Domain.Handlers;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Mappers;
using portfolio.Domain.Models;
using portfolio.Domain.Models.Command;
using portfolio.Domain.Queue;
using TTM.Shared.Models.PortfolioSimulation;
using TTM.Shared.Models.PortfolioSimulation.Command;

namespace portfolio.Controllers;

[Route("[controller]")]
[Produces("application/json")]
public class SimulationController(
    IProcessSimulationHandler simulationHandler,
    IRegisterSimulationHandler registerSimulationHandler,
    IQuerySimulationsHandler querySimulationsHandler,
    ISimulationRepository simulationRepository,
    SessionDateHandler sessionDateHandler,
    HistoricalExplorerQueueCache queue) : ControllerBase
{
    [HttpPost("process")]
    public async Task<ActionResult<SimulationDto>> Process([FromBody] SimulationCmd cmd)
    {
        SimulationDto simulationData = await simulationHandler.HandleProcessSimulationFromCmd(cmd);
        return simulationData;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterSimulationDto>> Register([FromBody] SimulationCmd cmd)
    {
        RegisterSimulationDto simulationData = await registerSimulationHandler.HandleRegisterSimulation(cmd);
        
        return simulationData;
    }

    [HttpGet]
    public List<SimulationDto> GetSimulationDtos([FromQuery] int limit)
    {
        return querySimulationsHandler.GetSimulations(limit);
    }

    [HttpPut("worker-session")]
    public async Task ToggleWorkerSessionDate()
    {
        await sessionDateHandler.ToggleSessionDate();
    }

    [HttpPut("historical-dates")]
    public IActionResult HistoricalCalculationSessionDates([FromBody] HistoricalCalculationSessionDatesCmd cmd)
    {
        if (cmd.NrIterations < 1)
        {
            return BadRequest("Number of iterations must be at least 1.");
        }
        
        foreach (var date in cmd.Dates)
        {
            queue.Enqueue(new HistoricalExplorerCalculationRequest(date, cmd.NrIterations, cmd.ProcessDirection));
        }
        return Ok();
    }

    [HttpGet("historical-dates")]
    public async Task<IActionResult> GetHistoricalCalculationSessionDates()
    {
        var mapper = new DtoMapper();
        var dates = await simulationRepository.GetAllSessionsWithCounts();
        var response = dates.Select(mapper.MapToSessionDto).ToList();
        return Ok(response);
    }

    [HttpGet("historical-queue")]
    public IActionResult GetHistoricalQueue()
    {
        var queueContents = queue.GetQueueContents();
        return Ok(queueContents);
    }

    [HttpGet("historical-queue/current")]
    public IActionResult GetCurrentlyRunningRequest()
    {
        var currentlyRunning = queue.GetCurrentlyRunning();
        return Ok(currentlyRunning);
    }
}