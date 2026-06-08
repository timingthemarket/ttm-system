namespace TTM.Shared.Models.PortfolioSimulation;

public class SimulationDto
{
    public Guid Id { get; set; }
    public DateTime? Completed { get; set; }
    public DateTime Registered { get; set; }
    public double? PercentageChange { get; set; }
    public decimal InitMoney { get; set; } 
    public List<SimulationPeriodDto> Periods { get; set; }
}