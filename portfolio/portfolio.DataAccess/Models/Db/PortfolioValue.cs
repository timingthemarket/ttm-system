using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace portfolio.DataAccess.Models.Db;

[Table("portfolio_value")]
public class PortfolioValue
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    [Column("security_id")]
    public long SecurityId { get; set; }
    [Column("weight")]
    public double Weight { get; set; }
    [Column("rank")]
    public long Rank { get; set; }
    [Column("amount")]
    public int Amount { get; set; }
    [Column("price")]
    public double Price { get; set; }
    [Column("portfolio_id")]
    public Guid PortfolioId { get; set; }

    public Portfolio Portfolio { get; set; } = null!;
}