using Microsoft.EntityFrameworkCore;

namespace securities_masterdata.DataAccess.Entities.Composite;

[Keyless]
public class AverageVolume
{
    public long SecurityId { get; set; }
    public double AvgVolume { get; set; }
}