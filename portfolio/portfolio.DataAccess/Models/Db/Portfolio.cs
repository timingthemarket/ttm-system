using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTM.Shared.Constants;

namespace portfolio.DataAccess.Models.Db;

[Table("portfolio")]
public class Portfolio
{
    [Key] [Column("id")] public Guid Id { get; set; }

    [Column("securities_date")] public DateOnly SecuritiesDate { get; set; }

    [Column("calculation_datetime")] public DateTime CalculationDate { get; set; }

    [Column("strategy")] public Strategy Strategy { get; set; }

    [MaxLength(100)]
    [Column("hash")]
    public string Hash { get; set; } = null!;

    [Column("row_similarity")]
    public double RowSimilarity { get; set; }
    
    //[InverseProperty("Portfolio")]
    //[ForeignKey("Id")]
    public SimulationPeriod SimulationPeriod { get; set; }

    public List<PortfolioValue> PortfolioValues { get; set; }
    public List<PortfolioIndicator> PortfolioIndicators { get; set; }
}