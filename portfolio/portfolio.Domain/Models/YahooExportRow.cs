using CsvHelper.Configuration.Attributes;

namespace portfolio.Domain.Models;

public class YahooExportRow
{
    [Name("Symbol")]
    public string Symbol { get; set; }
    [Name("Trade Date")]
    public string TradeDate { get; set; }
    [Name("Quantity")]
    public int Quantity { get; set; }
    [Name("Commission")]
    public int Commission { get; set; }
    [Name("Purchase Price")]
    public double PurchasePrice { get; set; }
}