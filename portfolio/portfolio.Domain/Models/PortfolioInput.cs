using TTM.Shared.Models.PortfolioSimulation;

namespace portfolio.Domain.Models;

public class PortfolioInput
{
    public required DateOnly Date { get; set; }
    public required long StrategyId { get; set; }
    public required double RowSimilarity { get; set; }
    public required decimal Money { get; set; }
    public required double MaxSecuritySpending { get; set; }
    /// <summary>
    /// {Key: sector name ; Value: weight of the sector}
    /// </summary>
    public Dictionary<string, double> SectorWeight { get; set; } = new();
    /// <summary>
    /// {Key: country name ; Value: weight of the country}
    /// </summary>
    public Dictionary<string, double> CountryWeight { get; set; } = new();
    public required List<PortfolioInputIndicatorVariable> Indicators { get; set; }
}

public class PortfolioInputIndicatorVariable : PortfolioSecurityIndicatorDto
{
    public StrategyImputation ImputationStrategy { get; set; } = new();
    
    public string ToStringRepresentation()
    {
        return $"{IndicatorId}|{LookBackPeriod?.Aggregate}|{LookBackPeriod?.Period}|{Direction}|{Weight}";
    }
}

