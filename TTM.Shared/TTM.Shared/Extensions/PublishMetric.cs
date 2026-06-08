using MassTransit;
using TTM.Shared.Events.Infra;

namespace TTM.Shared.Extensions;

public static class PublishMetric
{
    public static async Task Increment<T>(this ConsumeContext<T> context, string metricName)
        where T : class
    {
        var message = MakeEvent(1, metricName);
        await context.Publish(message, context => { context.MessageId = message.Id; });
    }

    public static async Task Increment(this IPublishEndpoint endpoint, string metricName)
    {
        var message = MakeEvent(1, metricName);
        await endpoint.Publish(message, context => { context.MessageId = message.Id; });
    }

    private static MetricEvent MakeEvent(double value, string metricName) => new ()
    {
        Id = Guid.NewGuid(),
        Value = value,
        Timestamp = DateTime.UtcNow,
        MetricName = metricName
    };
}