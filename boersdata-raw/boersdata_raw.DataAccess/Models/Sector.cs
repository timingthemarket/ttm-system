namespace boersdata_raw.DataAccess.Models;

public record Sector
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Translations Translations { get; set; } = new Translations();
    public List<Industry> Industries { get; set; } = new List<Industry>();
    public long SectorId { get; set; }
};
