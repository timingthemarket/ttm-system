using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using TTM.Shared.Events.Infra;
using TTM.Shared.Models.Infra;

namespace TTM.Shared.gRPC.Services;

[Service("LoggingService")]
public interface ILoggingService
{
    ValueTask SendSystemError(ErrorCmd error);
    
    ValueTask SendLogEvent(LogCmd log);
}