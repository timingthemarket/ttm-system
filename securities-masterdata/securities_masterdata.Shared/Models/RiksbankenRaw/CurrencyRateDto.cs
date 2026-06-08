namespace ttm_system.Shared.Models.RiksbankenRaw;

public class CurrencyRateDto
{
    public string FromCode { get; set; }
    public string ToCode { get; set; }
    public DateTime Date { get; set; }
    public double Rate { get; set; }
}