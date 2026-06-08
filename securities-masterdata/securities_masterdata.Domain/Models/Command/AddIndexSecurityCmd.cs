namespace securities_masterdata.Domain.Models.Command;

public class AddIndexSecurityCmd
{
    public long IndexId { get; set; }
    public List<IndexSecurityList> IndexSecurities { get; set; }
}

public class IndexSecurityList
{
    public long SecurityId { get; set; }
    public double Weight { get; set; }
}