using System.ComponentModel.DataAnnotations.Schema;
using TTM.Shared.Constants;

namespace securities_masterdata.DataAccess.Entities;

public class Indicator
{
    [Column("indicator_id")]
    public Indicators IndicatorId { get; set; }

    [Column("security_id")]
    public long SecurityId { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("value")]
    public decimal Value { get; set; }
}