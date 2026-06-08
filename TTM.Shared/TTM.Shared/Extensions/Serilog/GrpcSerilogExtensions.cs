using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace TTM.Shared.Extensions.Serilog;

public static class GrpcSerilogExtensions
{
    public static LoggerConfiguration Grpc(
        this LoggerSinkConfiguration sinkConfiguration,
        GrpcSinkConfiguration configuration,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Error)
    {
        return sinkConfiguration.Sink(
            new GrpcSerilogSink(configuration, restrictedToMinimumLevel),
            restrictedToMinimumLevel);
    }
}