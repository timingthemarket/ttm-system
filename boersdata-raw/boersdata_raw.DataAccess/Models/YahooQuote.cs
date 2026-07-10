namespace boersdata_raw.DataAccess.Models;

public class YahooQuote
{
    public long Id { get; set; }
    public string YahooTicker { get; set; }
    public string? Currency { get; set; }
    public string? FinancialCurrency { get; set; }
    public long BoersDataInstrumentId { get; set; }
}
