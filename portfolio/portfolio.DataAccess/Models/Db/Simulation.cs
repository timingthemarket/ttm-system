using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace portfolio.DataAccess.Models.Db;

[Table("simulation")]
public class Simulation
{
    [Key] [Column("id")] public Guid Id { get; set; }

    [Column("completed")] public DateTime? Completed { get; set; }

    [Column("registered")] public DateTime Registered { get; set; }

    [Column("percentage_change")] public double? PercentageChange { get; set; }

    [Column("init_money")] public decimal InitMoney { get; set; }

    [Column("session_id")]
    public int SessionId { get; set; }
    [ForeignKey("SessionId")] public Session Session { get; set; } = null!;
    [InverseProperty("Simulation")] public List<SimulationPeriod> Periods { get; set; } = null!;
}
