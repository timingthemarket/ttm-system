using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTM.Shared.Constants;

namespace portfolio.DataAccess.Models.Db;

[Table("indicator_strength")]
public class IndicatorStrength
{
    [Key] [Column("id")] public int Id { get; set; }

    [Column("indicator_id")] public Indicators IndicatorId { get; set; }

    /// <summary>
    /// Which side of the indicator was tested. Volatility and RsiMomentum are evaluated
    /// both as High and as Low, so the direction is part of the identity of a row.
    /// </summary>
    [Column("direction")] public Direction Direction { get; set; }

    [Column("date")] public DateOnly Date { get; set; }

    /// <summary>
    /// Annualised rolling Sharpe ratio of the artificial portfolio built from this indicator.
    /// </summary>
    [Column("sharpe_ratio")] public double SharpeRatio { get; set; }

    /// <summary>
    /// Mean Information Coefficient over the same rolling window, or null when no period in the
    /// window produced one.
    /// </summary>
    [Column("ic")] public double? Ic { get; set; }
}
