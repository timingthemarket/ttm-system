using System.ComponentModel.DataAnnotations.Schema;

namespace portfolio.DataAccess.Models.Views;

public class SimulationView
{
    [Column("id")] public Guid Id { get; set; }

    [Column("completed")] public DateTime? Completed { get; set; }

    [Column("registered")] public DateTime Registered { get; set; }

    [Column("percentage_change")] public double? PercentageChange { get; set; }

    [Column("init_money")] public decimal InitMoney { get; set; }
    [Column("securities_date")] public DateOnly SecuritiesDate { get; set; }
}