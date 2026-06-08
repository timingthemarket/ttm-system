namespace TTM.Shared.Events.BoersDataRaw;

public class HistoricalPricesSyncCompleteEvent
{
    public List<string>? Tickers { get; set; }
}