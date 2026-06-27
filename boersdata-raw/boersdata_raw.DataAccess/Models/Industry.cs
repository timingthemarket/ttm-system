namespace boersdata_raw.DataAccess.Models;

public record Industry
{
    public string Name { get; set; } = string.Empty;
    public Translations Translations { get; set; } = new Translations();
    public long IndustryId { get; set; }
};