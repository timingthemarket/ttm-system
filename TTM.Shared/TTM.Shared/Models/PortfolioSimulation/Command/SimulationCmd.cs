using TTM.Shared.Constants;

namespace TTM.Shared.Models.PortfolioSimulation.Command;

public class SimulationCmd
{
    public decimal InitMoney { get; set; }
    public required double RowSimilarityLimit { get; set; }
    public required List<SimulationPeriodsCmd> Periods { get; set; }
    public DateOnly? DateSimulationEnd { get; set; }
}

public class SimulationPeriodsCmd
{
    public required long StrategyId { get; set; }
    public required DateOnly DateStart { get; set; }
    public double MaxSecuritySpending { get; set; }
    public required List<FinancialVariablesCmd> Variables { get; set; }
}

public class FinancialVariablesCmd
{
    public required Direction Direction { get; set; }
    public required Indicators IndicatorId { get; set; }
    public double? Weight { get; set; }
    public LookBackPeriod? LookBackPeriod { get; set; }
}