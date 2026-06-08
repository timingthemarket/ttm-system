using System.Text.Json;
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using Serilog.Core;
using Serilog.Events;
using TTM.Shared.Events.Infra;
using TTM.Shared.gRPC.Services;
using TTM.Shared.Models.Infra;

namespace TTM.Shared.Extensions.Serilog;

public class GrpcSerilogSink : ILogEventSink
{
    private readonly GrpcChannel _channel;
    private readonly ILoggingService _loggingService;
    private readonly string _serviceName;
    private readonly LogEventLevel _minimumLevel;

    public GrpcSerilogSink(GrpcSinkConfiguration configuration, LogEventLevel minimumLevel = LogEventLevel.Error)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));
        
        if (string.IsNullOrEmpty(configuration.ServerUrl))
            throw new ArgumentException("ServerUrl cannot be null or empty", nameof(configuration));
        
        if (string.IsNullOrEmpty(configuration.ServiceName))
            throw new ArgumentException("ServiceName cannot be null or empty", nameof(configuration));

        _serviceName = configuration.ServiceName;
        _minimumLevel = minimumLevel;
        
        GrpcClientFactory.AllowUnencryptedHttp2 = true;
        
        _channel = GrpcChannel.ForAddress(configuration.ServerUrl);
        _loggingService = _channel.CreateGrpcService<ILoggingService>();
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level < _minimumLevel)
            return;

        try
        {
            if (logEvent.Level >= LogEventLevel.Error)
            {
                var errorMessage = new ErrorCmd
                {
                    Timestamp = logEvent.Timestamp.UtcDateTime,
                    Message = logEvent.RenderMessage(),
                    Service = _serviceName,
                    StackTrace = logEvent.Exception?.StackTrace,
                    SpanId = logEvent.SpanId.ToString(),
                    TraceId = logEvent.TraceId.ToString(),
                };

                _ = _loggingService.SendSystemError(errorMessage);
            }
            else
            {
                var logMessage = new LogCmd
                {
                    Timestamp = logEvent.Timestamp.UtcDateTime,
                    Message = logEvent.RenderMessage(),
                    Service = _serviceName,
                    SpanId = logEvent.SpanId.ToString(),
                    TraceId = logEvent.TraceId.ToString(),
                    Metadata = JsonSerializer.Serialize(new
                        { logEvent.MessageTemplate }) 
                };

                _ = _loggingService.SendLogEvent(logMessage);
            }
        }
        catch
        {
            // Sink should not throw exceptions
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}