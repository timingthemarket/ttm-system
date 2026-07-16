using TTM.Shared.Models.PortfolioSimulation;

namespace portfolio.Domain.Models;

public class SecuritiesPortfolioQry
{
    public DateOnly Date { get; set; }
    public long StrategyId { get; set; }
    public double RowSimilarityLimit { get; set; }
    public decimal Money { get; set; }
    public double MaxSecuritySpending { get; set; }
    public required List<PortfolioInputIndicatorVariable> Variables { get; set; }
}