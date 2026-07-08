namespace portfolio.Domain.Models.Events;

public class HistoricalExplorerCalculationEvent
{
    public DateOnly SessionDate { get; set; }
    public int NrIterations { get; set; }
}