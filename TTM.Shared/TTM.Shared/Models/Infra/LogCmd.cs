using System.Runtime.Serialization;

namespace TTM.Shared.Models.Infra;

[DataContract]
public class LogCmd
{
    [DataMember(Order = 1)]
    public DateTime Timestamp { get; set; }
    [DataMember(Order = 2, IsRequired = true)]
    public string Message { get; set; }
    [DataMember(Order = 3, IsRequired = true)]
    public string Service { get; set; }
    [DataMember(Order = 4)]
    public string? Metadata { get; set; }

    [DataMember(Order = 5)]
    public string? SpanId { get; set; }
    [DataMember(Order = 6)]
    public string? TraceId { get; set; }
}