using System.ComponentModel.DataAnnotations.Schema;

namespace securities_masterdata.DataAccess.Entities;

public class IndexValue
{
    [Column("index_id")]
    public long IndexId { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("value")]
    public decimal Value { get; set; }

    public Index Index { get; set; }
}