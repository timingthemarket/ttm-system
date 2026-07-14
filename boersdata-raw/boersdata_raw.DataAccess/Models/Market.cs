namespace boersdata_raw.DataAccess.Models;

public sealed record Market
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long MarketId { get; set; }
}
