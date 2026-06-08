using MassTransit;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;

namespace TTM.Shared.Filters;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class ExceptionFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private long _attemptCount;
    private long _exceptionCount;
    private long _successCount;

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        try
        {
            Interlocked.Increment(ref _attemptCount);
            await next.Send(context);
            Interlocked.Increment(ref _successCount);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _exceptionCount);
            await context.SendSystemError(ex, SharedSettings.AppName);
            throw;
        }
    }

    public void Probe(ProbeContext context)
    {
        var scope = context.CreateFilterScope("exceptionLogger");
        scope.Add("attempted", _attemptCount);
        scope.Add("succeeded", _successCount);
        scope.Add("faulted", _exceptionCount);
    }
}