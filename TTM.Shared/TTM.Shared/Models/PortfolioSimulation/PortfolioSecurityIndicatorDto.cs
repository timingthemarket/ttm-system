using TTM.Shared.Constants;

namespace TTM.Shared.Models.PortfolioSimulation;

public class PortfolioSecurityIndicatorDto
{
    public required Direction Direction { get; set; }
    public Indicators IndicatorId { get; set; }
    public double? Weight { get; set; }
    public LookBackPeriod? LookBackPeriod { get; set; }
}