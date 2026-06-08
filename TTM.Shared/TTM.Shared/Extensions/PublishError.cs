using MassTransit;
using TTM.Shared.Events.Infra;

namespace TTM.Shared.Extensions;

public static class PublishError
{
    public static async Task SendSystemError(this IBus bus, Exception ex, string service)
    {
        var message = MakeErrorEvent(ex, service);
        await bus.Publish(message, context => { context.MessageId = message.Id; });
    }

    public static async Task SendSystemError<T>(this ConsumeContext<T> context, Exception ex, string service)
        where T : class
    {
        var message = MakeErrorEvent(ex, service);
        await context.Publish(message, cont => {
            cont.MessageId = message.Id;
            cont.CorrelationId = context.CorrelationId;
        });
    }

    public static async Task SendSystemError(this IPublishEndpoint endpoint, Exception ex, string service)
    {
        var message = MakeErrorEvent(ex, service);
        await endpoint.Publish(message, context => { context.MessageId = message.Id; });
    }

    private static SystemErrorEvent MakeErrorEvent(Exception ex, string service)
    {
        var errorId = NewId.NextSequentialGuid();
        return new SystemErrorEvent
        {
            Id = errorId,
            Timestamp = DateTime.UtcNow,
            StackTrace = ex.StackTrace,
            Message = ex.Message,
            Service = service
        };
    }
}