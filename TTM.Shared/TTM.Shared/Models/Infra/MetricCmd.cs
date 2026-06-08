using System.Runtime.Serialization;

namespace TTM.Shared.Models.Infra;

[DataContract]
public class MetricCmd
{
    [DataMember(Order = 1, IsRequired = true)]
    public DateTime Timestamp { get; set; }
    [DataMember(Order = 2, IsRequired = true)]
    public double Value { get; set; }
    [DataMember(Order = 3, IsRequired = true)]
    public string MetricName { get; set; }
}