using TTM.Shared.Constants;

namespace TTM.Shared.Events.PortfolioSimulation;

public class PortfolioNotificationEvent
{
    public SimulationMetadata? SimulationMetadata { get; set; }
    public PortfolioMetadata? PortfolioMetadata { get; set; }
}

public class PortfolioMetadata
{
    public Guid PortfolioId { get; set; }
    public PortfolioState State { get; set; }
}

public class SimulationMetadata
{
    public Guid SimulationId { get; set; }
    public double PercentageValueIncrease { get; set; }
    public SimulationState State { get; set; }
}