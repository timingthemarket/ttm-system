using portfolio.Domain.Constants;
using TTM.Shared.Constants;
using TTM.Shared.Models;

namespace portfolio.Domain.Models;

public record ProcessSimulation
{
    public Guid Id { get; set; }
    public DateTime RegistrationCreated { get; set; }
    public DateOnly? DateSimulationEnd { get; set; }
    public required decimal InitMoney { get; set; }
    /// <summary>
    /// {Key: sector name ; Value: weight of the sector}
    /// </summary>
    public Dictionary<string, double> SectorWeight { get; set; } = new();
    /// <summary>
    /// {Key: country name ; Value: weight of the country}
    /// </summary>
    public Dictionary<string, double> CountryWeight { get; set; } = new();
    public double RowSimilarityLimit { get; set; }
    public List<SimulationPeriod> Periods { get; set; } = null!;
}

public record SimulationPeriod
{
    public required long StrategyId { get; set; }
    public required double MaxSecuritySpending { get; set; }
    public required DateOnly DateStart { get; set; }
    public required List<SimulationFinancialVariable> Variables { get; set; }
}

public record SimulationFinancialVariable
{
    public Direction Direction { get; set; }
    public Indicators IndicatorId { get; set; }
    public double? Weight { get; set; }
    public LookBackPeriod? LookBackPeriod { get; set; }
}