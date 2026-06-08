using System.ComponentModel.DataAnnotations.Schema;

namespace securities_masterdata.DataAccess.Entities;

public class IndexSecurity
{
    [Column("index_id")]
    public long IndexId { get; set; }

    [Column("security_id")]
    public long SecurityId { get; set; }

    [Column("weight")]
    public double Weight { get; set; }

    public Index Index { get; set; }
    public Security Security { get; set; }
}