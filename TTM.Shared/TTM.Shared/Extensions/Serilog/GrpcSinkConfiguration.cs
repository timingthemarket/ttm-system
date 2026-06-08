namespace TTM.Shared.Extensions.Serilog;

public class GrpcSinkConfiguration
{
    public string ServerUrl { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
}