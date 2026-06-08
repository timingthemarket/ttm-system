using System.ComponentModel.DataAnnotations.Schema;

namespace securities_masterdata.DataAccess.Entities;

public class Index
{
    [Column("index_id")]
    public long IndexId { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("base_date")]
    public DateOnly BaseDate { get; set; }
    
    public List<IndexValue> IndexValues { get; set; }
    public List<IndexSecurity> IndexSecurities { get; set; }
}