using System.Runtime.Serialization;

namespace TTM.Shared.Events.Infra;

public class LoggingEvent
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; }
    public string Service { get; set; }
    public object? Metadata { get; set; }
}