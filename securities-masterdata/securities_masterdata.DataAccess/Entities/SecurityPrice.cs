using System.ComponentModel.DataAnnotations.Schema;

namespace securities_masterdata.DataAccess.Entities;

public class SecurityPrice
{
    [Column("security_id")]
    public long SecurityId { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("open")]
    public double Open { get; set; }

    [Column("high")]
    public double High { get; set; }

    [Column("low")]
    public double Low { get; set; }

    [Column("close")]
    public double Close { get; set; }

    [Column("volume")]
    public long Volume { get; set; }

    public Security Security { get; set; }
}