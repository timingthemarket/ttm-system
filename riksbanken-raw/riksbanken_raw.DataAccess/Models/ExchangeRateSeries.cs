namespace riksbanken_raw.DataAccess.Models;

public class ExchangeRateSeries
{
    public Guid Id { get; set; }
    public string SeriesId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public DateTime? LastFetched { get; set; }
}
