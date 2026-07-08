using System.ComponentModel.DataAnnotations.Schema;

namespace portfolio.DataAccess.Models.Db;

[Table("session")]
public class Session
{
    [Column("id")]
    public int Id { get; set; }

    [Column("session_date")]
    public DateOnly SessionDate { get; set; }

    [InverseProperty("Session")] public List<Simulation> Simulations { get; set; } = null!;
}