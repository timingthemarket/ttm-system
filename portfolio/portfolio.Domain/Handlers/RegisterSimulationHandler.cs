using System.Text;
using Microsoft.Extensions.Logging;
using portfolio.Domain.Interfaces;
using portfolio.Domain.Models;
using portfolio.Domain.Queue;
using portfolio.Domain.Utils;
using TTM.Shared.Models.PortfolioSimulation;
using TTM.Shared.Models.PortfolioSimulation.Command;

namespace portfolio.Domain.Handlers;

public class RegisterSimulationHandler(ILogger<RegisterSimulationHandler> logger, SimulationQueueCache queueCache)
    : IRegisterSimulationHandler
{
    public Task<RegisterSimulationDto> HandleRegisterSimulation(SimulationCmd evt)
    {
        var id = Guid.NewGuid();
        ProcessSimulation simulationDto = MapSimulationQueueDto(id, evt);
        queueCache.Enqueue(simulationDto);

        string log = MakeLogMessage(simulationDto);
        logger.LogInformation("Registered simulation. \nId: {Id}\nContent:\n {Log}", id, log);

        return Task.FromResult(MapRegisterSimulationDto(id));
    }

    private RegisterSimulationDto MapRegisterSimulationDto(Guid id) => new()
    {
        SimulationId = id
    };

    private string MakeLogMessage(ProcessSimulation dto)
    {
        var sb = new StringBuilder();
        foreach (SimulationPeriod period in dto.Periods)
            sb.AppendLine($"Period: {period.DateStart}::[{StringUtils.GetIndicatorsString(period.Variables)}]");

        return sb.ToString();
    }

    private ProcessSimulation MapSimulationQueueDto(Guid id, SimulationCmd evt)
    {
        return new ProcessSimulation
        {
            Id = id,
            DateSimulationEnd = evt.DateSimulationEnd,
            RegistrationCreated = DateTime.UtcNow,
            RowSimilarityLimit = evt.RowSimilarityLimit,
            InitMoney = evt.InitMoney,
            Periods = evt.Periods.Select(p => new SimulationPeriod
            {
                StrategyId = p.StrategyId,
                DateStart = p.DateStart,
                MaxSecuritySpending = p.MaxSecuritySpending,
                Variables = p.Variables.Select(v => new SimulationFinancialVariable
                {
                    Direction = v.Direction,
                    IndicatorId = v.IndicatorId,
                    Weight = v.Weight,
                    LookBackPeriod = v.LookBackPeriod
                }).ToList()
            }).ToList()
        };
    }
}