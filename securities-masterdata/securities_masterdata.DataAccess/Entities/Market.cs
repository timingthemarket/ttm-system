using System.ComponentModel.DataAnnotations.Schema;

namespace securities_masterdata.DataAccess.Entities;

public class Market
{
    [Column("market_id")]
    public long MarketId { get; set; }

    [Column("name")]
    public string Name { get; set; }
    /// <summary>
    /// UTC time
    /// </summary>
    [Column("open_time")]
    public TimeOnly? OpenTime { get; set; }
    /// <summary>
    /// UTC time
    /// </summary>
    [Column("close_time")]
    public TimeOnly? CloseTime { get; set; }

    [Column("updated")]
    public DateTime Updated { get; set; }

    [Column("inactive")] 
    public bool Inactive { get; set; }

    public Security Security { get; set; }
}