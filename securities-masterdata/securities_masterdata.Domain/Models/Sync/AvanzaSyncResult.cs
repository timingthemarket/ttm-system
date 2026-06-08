namespace securities_masterdata.Domain.Models.Sync;

public class AvanzaSyncResult
{
    public int TotalSecuritiesInDatabase { get; set; }
    public int TotalAvanzaStocks { get; set; }
    public int SecuritiesMarkedInactive { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SyncDateTime { get; set; } = DateTime.UtcNow;
}