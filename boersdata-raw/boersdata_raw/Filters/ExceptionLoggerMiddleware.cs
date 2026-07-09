using MassTransit;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;

namespace boersdata_raw.Filters;

public class ExceptionLoggerMiddleware(
    RequestDelegate next,
    IBus publishEndpoint,
    ILogger<ExceptionLoggerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await publishEndpoint.SendSystemError(ex, SharedSettings.AppName);
            logger.LogError(ex, "Exception occured");
        }
    }
}