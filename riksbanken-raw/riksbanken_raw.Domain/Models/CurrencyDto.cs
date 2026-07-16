namespace riksbanken_raw.Domain.Models;

public record Rate(DateTime Date, double Value);

public class CurrencyDto
{
    public IEnumerable<Rate> Rates { get; set; }
    public string FromCode { get; set; }
    public string ToCode { get; set; }
}