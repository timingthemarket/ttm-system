using System.ComponentModel.DataAnnotations.Schema;

namespace securities_masterdata.DataAccess.Entities;

public class Security
{
    [Column("security_id")]
    public long SecurityId { get; set; }

    [Column("ticker")]
    public string Ticker { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("isin")]
    public string? Isin { get; set; }

    [Column("market_id")]
    public long MarketId { get; set; }

    [Column("currency_id")]
    public long CurrencyId { get; set; }

    [Column("industry")]
    public string? Industry { get; set; }

    [Column("sector")]
    public string Sector { get; set; }

    [Column("country")]
    public string Country { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("yahoo_ticker")] public string? YahooTicker { get; set; }

    [Column("inactive")]
    public bool Inactive { get; set; }

    [Column("updated")]
    public DateTime Updated { get; set; }

    public List<SecurityPrice> SecuritiesPrices { get; set; }
    public Market Market { get; set; }
    public Currency Currency { get; set; }
    public IndexSecurity IndexSecurity { get; set; }
}