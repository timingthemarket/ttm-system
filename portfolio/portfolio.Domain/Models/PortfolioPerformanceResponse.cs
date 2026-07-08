namespace portfolio.Domain.Models;

public class PortfolioPerformanceResponse
{
    public string SetId { get; set; } = null!;
    public DateOnly OriginalDate { get; set; }
    public DateOnly TargetDate { get; set; }
    public double OriginalValue { get; set; }
    public double CurrentValue { get; set; }
    public double PercentageChange { get; set; }
}
