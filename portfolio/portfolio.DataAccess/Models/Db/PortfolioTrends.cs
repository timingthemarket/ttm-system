using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace portfolio.DataAccess.Models.Db;

[Table("portfolio_trends")]
public class PortfolioTrends
{
    [Key] [Column("id")] public Guid Id { get; set; }

    [Column("set_id")] public string SetId { get; set; }
    
    [Column("revision")] public int Revision { get; set; }

    [Column("beta0")] public double? Beta0 { get; set; }

    [Column("beta1")] public double? Beta1 { get; set; }

    [Column("beta2")] public double? Beta2 { get; set; }

    [Column("timestamp")] public DateTime Timestamp { get; set; }
}