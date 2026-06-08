using System.ComponentModel.DataAnnotations.Schema;

namespace securities_masterdata.DataAccess.Entities;

public class Currency
{
    [Column("currency_id")]
    public long CurrencyId { get; set; }

    [Column("currency_code")]
    public string CurrencyCode { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("updated")]
    public DateTime Updated { get; set; }

    public Security Security { get; set; }

    public List<CurrencyRate> CurrencyRates { get; set; }
}