using MassTransit;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;

namespace article_news_raw.Filters;

public class ExceptionLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IBus _publishEndpoint;

    public ExceptionLoggerMiddleware(RequestDelegate next, IBus publishEndpoint)
    {
        _next = next;
        _publishEndpoint = publishEndpoint;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await _publishEndpoint.SendSystemError(ex, SharedSettings.AppName);
        }
    }
}