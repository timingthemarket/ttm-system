using Grpc.Core;
using Grpc.Core.Interceptors;
using MassTransit;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;

namespace securities_masterdata.Filters;

public class InterceptorHandler(IPublishEndpoint publishEndpoint) : Interceptor
{
    public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
        IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        var head = context.RequestHeaders;
        var bearer = head.Get("Authorization");

        try
        {
            return continuation(request, responseStream, context);
        }
        catch (Exception e)
        {
            publishEndpoint.SendSystemError(e, SharedSettings.AppName);
            throw;
        }
    }


    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var head = context.RequestHeaders;
        var bearer = head.Get("Authorization");

        try
        {
            return continuation(request, context);
        }
        catch (Exception e)
        {
            publishEndpoint.SendSystemError(e, SharedSettings.AppName);
            throw;
        }
    }
}