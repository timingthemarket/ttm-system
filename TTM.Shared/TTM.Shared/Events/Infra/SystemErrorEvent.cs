using System.Runtime.Serialization;

namespace TTM.Shared.Events.Infra;

public class SystemErrorEvent
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; }
    public string Service { get; set; }
    public string StackTrace { get; set; }
}