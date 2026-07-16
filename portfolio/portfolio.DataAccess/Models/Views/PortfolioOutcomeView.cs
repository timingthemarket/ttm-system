using System.ComponentModel.DataAnnotations.Schema;

namespace portfolio.DataAccess.Models.Views;

public class PortfolioOutcomeView
{
    [Column("portfolio_id")] public Guid PortfolioId { get; set; }

    [Column("session_date")] public DateOnly SessionDate { get; set; }

    [Column("set_id")] public string SetId { get; set; } = null!;

    [Column("percentage_change")] public double PercentageChange { get; set; }
}