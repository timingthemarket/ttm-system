using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTM.Shared.Constants;

namespace portfolio.DataAccess.Models.Db;

[Table("portfolio_indicators")]
public class PortfolioIndicator
{
    [Column("portfolio_id")] public Guid PortfolioId { get; set; }
    [Column("indicator")] public Indicators Indicator { get; set; }
    [Column("weight")] public double Weight { get; set; }
    [Column("direction")] public Direction Direction { get; set; }
    
    [MaxLength(100)]
    [Column("lookback")] public required string LookBack { get; set; }

    [Column("lookback_period")]
    public int? LookbackPeriod { get; set; }
    
    [Column("lookback_aggregator")]
    public Aggregator? LookbackAggregator { get; set; }

    public Portfolio Portfolio { get; set; } = null!;
}