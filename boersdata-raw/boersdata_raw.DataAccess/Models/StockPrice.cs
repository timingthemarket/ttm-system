namespace boersdata_raw.DataAccess.Models;

public record StockPrice
{
    public Guid Id { get; set; }

    public long InsId { get; set; }
    public string Ticker { get; set; }
    public double? Open { get; set; }
    public double? Close { get; set; }
    public double? High { get; set; }
    public double? Low { get; set; }
    public long? Volume { get; set; }
    public DateTime Date { get; set; }
}
