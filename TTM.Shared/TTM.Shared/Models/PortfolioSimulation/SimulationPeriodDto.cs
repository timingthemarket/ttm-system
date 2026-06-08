namespace TTM.Shared.Models.PortfolioSimulation;

public class SimulationPeriodDto
{ 
    public Guid Id { get; set; }
    public decimal InitMoney { get; set; }
    public decimal InvestedMoney { get; set; }
    public decimal LiquidMoney { get; set; }
    public PortfolioDto Portfolio { get; set; }
}