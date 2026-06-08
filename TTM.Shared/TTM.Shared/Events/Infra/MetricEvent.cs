using System.Runtime.Serialization;

namespace TTM.Shared.Events.Infra;

public class MetricEvent
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string MetricName { get; set; }
}