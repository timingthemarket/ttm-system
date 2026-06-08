using System.ComponentModel.DataAnnotations.Schema;

namespace securities_masterdata.DataAccess.Entities;

public class CurrencyRate
{
    [Column("currency_id_from")]
    public long CurrencyIdFrom { get; set; }

    [Column("currency_id_to")]
    public long CurrencyIdTo { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("rate")]
    public double Rate { get; set; }

    public Currency Currency { get; set; }
}