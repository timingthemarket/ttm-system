namespace boersdata_raw.DataAccess.Models;

public record Country
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Translations Translations { get; set; } = new Translations();
    public long CountryId { get; set; }
};
