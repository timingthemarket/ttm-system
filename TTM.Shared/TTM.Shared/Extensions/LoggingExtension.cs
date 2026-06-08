using MassTransit;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.OpenTelemetry;
using TTM.Shared.Constants;
using TTM.Shared.Extensions.Serilog;

namespace TTM.Shared.Extensions;

public static class LoggingExtension
{
    public static ILoggingBuilder AddTtmLogger(this ILoggingBuilder builder, out Logger logger, Action<
        LoggerConfiguration>? conf = null)
    {
        var loggerConf = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console(LogEventLevel.Information);

        if (conf != null)
            conf(loggerConf);

        logger = loggerConf.CreateLogger();
        
        return builder
            .AddSerilog(logger);
    }

    public static ILoggingBuilder AddTtmOtelLogger(this ILoggingBuilder builder, string oltpEndpoint, string appName,
        Action<
            LoggerConfiguration>? conf = null)
    {
        var loggerConf = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning);

        if (conf != null)
            conf(loggerConf);

        var logger = loggerConf
            .WriteTo.OpenTelemetry(opt =>
            {
                opt.Endpoint = oltpEndpoint;
                opt.Protocol = OtlpProtocol.Grpc;
                opt.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = appName,
                    ["flag"] = true,
                    ["service.instance.id"] = Environment.MachineName,
                    ["deployment.environment"] = "production",
                    ["service.version"] = SystemVariables.Version
                };
            })
            .WriteTo.Console(new CompactJsonFormatter(), LogEventLevel.Information)
            .CreateLogger();

        return builder
            .AddSerilog(logger);
    }
}