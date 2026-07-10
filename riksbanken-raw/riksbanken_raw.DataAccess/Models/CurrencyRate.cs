namespace riksbanken_raw.DataAccess.Models;

public class CurrencyRate
{
    public Guid Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public double Rate { get; set; }
    public string FromCode { get; set; } = string.Empty;
    public string ToCode { get; set; } = string.Empty;
}
