using MassTransit;
using TTM.Shared.Constants;
using TTM.Shared.Extensions;

namespace portfolio.Filters;

public class ExceptionLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IBus _publishEndpoint;
    private readonly ILogger<ExceptionLoggerMiddleware> _logger;

    public ExceptionLoggerMiddleware(RequestDelegate next, IBus publishEndpoint, ILogger<ExceptionLoggerMiddleware> logger)
    {
        _next = next;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 400;
            await _publishEndpoint.SendSystemError(ex, SharedSettings.AppName);
            _logger.LogError(ex, "{Message}", ex.Message);
        }
    }
}