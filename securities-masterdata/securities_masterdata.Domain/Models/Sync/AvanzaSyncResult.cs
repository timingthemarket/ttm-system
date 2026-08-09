namespace securities_masterdata.Domain.Models.Sync;

public class AvanzaSyncResult
{
    public int TotalSecuritiesInDatabase { get; set; }
    public int TotalAvanzaStocks { get; set; }
    public int TotalNordnetStocks { get; set; }

    /// <summary>Securities that were active but are on neither platform, so are now inactive.</summary>
    public int SecuritiesMarkedInactive { get; set; }

    /// <summary>Securities that were inactive but showed up on a platform again, so are now active.</summary>
    public int SecuritiesMarkedActive { get; set; }

    public int SecuritiesOnAvanzaOnly { get; set; }
    public int SecuritiesOnNordnetOnly { get; set; }
    public int SecuritiesOnBothPlatforms { get; set; }

    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SyncDateTime { get; set; } = DateTime.UtcNow;
}
