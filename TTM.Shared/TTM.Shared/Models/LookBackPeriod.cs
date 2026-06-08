using System.Runtime.Serialization;
using TTM.Shared.Constants;

namespace TTM.Shared.Models;

[DataContract]
public class LookBackPeriod
{
    [DataMember(Order = 1, IsRequired = true)]
    public int Period { get; set; }

    [DataMember(Order = 2, IsRequired = true)]
    public Aggregator Aggregate { get; set; } = Aggregator.Value;
}