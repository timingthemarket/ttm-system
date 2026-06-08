using MassTransit.Logging;
using MassTransit.Monitoring;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TTM.Shared.Constants;

namespace TTM.Shared.Extensions;

public static class OtelExtension
{
    public static MeterProviderBuilder AddTtmMetrics(this MeterProviderBuilder builder, string oltpEndpoint, string appName)
    {
        return builder
            .ConfigureResource(resource =>
                resource.AddService(
                        serviceName: appName,
                        serviceInstanceId: Environment.MachineName,
                        serviceVersion: SystemVariables.Version))
            .AddMeter(InstrumentationOptions.MeterName, "Microsoft.AspNetCore.Hosting",
                "Microsoft.AspNetCore.Server.Kestrel", "Microsoft.Extensions.Diagnostics.ResourceMonitoring")
            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri(oltpEndpoint);
                opt.Protocol = OtlpExportProtocol.Grpc;
            });
    }

    public static TracerProviderBuilder AddTtmTracing(this TracerProviderBuilder builder, string oltpEndpoint,
        string appName)
    {
        return builder
            .AddSource(SharedSettings.AppName)
            .ConfigureResource(resource =>
                resource.AddService(
                    serviceName: appName,
                    serviceInstanceId: Environment.MachineName,
                    serviceVersion: SystemVariables.Version))
            .AddSource(DiagnosticHeaders.DefaultListenerName)
            .AddEntityFrameworkCoreInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddGrpcClientInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri(oltpEndpoint);
                opt.Protocol = OtlpExportProtocol.Grpc;
            });
    }
}