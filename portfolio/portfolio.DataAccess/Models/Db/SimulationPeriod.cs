using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace portfolio.DataAccess.Models.Db;

[Table("simulation_period")]
public class SimulationPeriod
{ 
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("invested_money")]
    public decimal InvestedMoney { get; set; }
    [Column("liquid_money")]
    public decimal LiquidMoney { get; set; }
    
    [Column("simulation_id")]
    public Guid SimulationId { get; set; }
    [Column("portfolio_id")]
    public Guid PortfolioId { get; set; }
    
    //[ForeignKey("PortfolioId")]
    //[InverseProperty("SimulationPeriod")]
    public Portfolio Portfolio { get; set; } = null!;
    [ForeignKey("SimulationId")]
    public Db.Simulation Simulation { get; set; } = null!;
}