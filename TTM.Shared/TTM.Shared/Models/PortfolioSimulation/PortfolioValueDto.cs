namespace TTM.Shared.Models.PortfolioSimulation;

public class PortfolioValueDto
{
    public long SecurityId { get; set; }
    public double Weight { get; set; }
    public long Rank { get; set; }
    public int Amount { get; set; }
    
    /// <summary>
    /// Price for a single security
    /// </summary>
    public double Price { get; set; }
}
