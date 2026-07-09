using Grpc.Core;
using Grpc.Core.Interceptors;
using MassTransit;
using TTM.Shared.Extensions;

namespace boersdata_raw.Filters;

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
            publishEndpoint.SendSystemError(e, nameof(boersdata_raw));
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
            publishEndpoint.SendSystemError(e, nameof(boersdata_raw));
            throw;
        }
    }
}