using TTM.Shared.Constants;
using TTM.Shared.Models;

namespace portfolio.Domain.Models;

public class StrategyInput
{
    public required decimal Money { get; set; }
    public DateOnly Date { get; set; }
    public double RowSimilarityLimit { get; set; }
    public required double MaxSecuritySpending { get; set; }
    public required string Hash { get; set; }
    /// <summary>
    /// {Key: sector name ; Value: weight of the sector}
    /// </summary>
    public Dictionary<string, double> SectorWeight { get; set; } = new();
    /// <summary>
    /// {Key: country name ; Value: weight of the country}
    /// </summary>
    public Dictionary<string, double> CountryWeight { get; set; } = new();

    public List<StrategyInputVariable> StrategyVariables { get; set; } = null!;

    public decimal MaxCountryMoney
    {
        get
        {
            if (Money >= 190_000)
            {
                return Money * 0.1M;
            } 
            if (Money >= 95_000)
            {
                return Money * 0.2M;
            }
            if (Money >= 45_000)
            {
                return Money * 0.3M;
            }
            
            return Money * 0.5M;
        }
    }
}

public class StrategyInputVariable
{
    public Indicators IndicatorId { get; set; }
    public LookBackPeriod? LookBackPeriod { get; set; }
    public Direction Direction { get; set; }
    public double? Weight { get; set; }
    public StrategyImputation Imputation { get; set; } = new();
}

public class StrategyImputation
{
    public MissingDataAction Action { get; set; } = MissingDataAction.Remove;
    public decimal? ImputationValue { get; set; }
}

public enum MissingDataAction
{
    Remove,
    Value,
    Worst
}
