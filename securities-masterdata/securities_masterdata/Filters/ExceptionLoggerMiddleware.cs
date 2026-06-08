using MassTransit;
using TTM.Shared.Extensions;

namespace securities_masterdata.Filters;

public class ExceptionLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IBus _publishEndpoint;
    private readonly ILogger<ExceptionLoggerMiddleware> _logger;

    public ExceptionLoggerMiddleware(RequestDelegate next, IBus publishEndpoint,
        ILogger<ExceptionLoggerMiddleware> logger)
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
            _logger.LogError(ex, ex.Message);
            await _publishEndpoint.SendSystemError(ex, nameof(securities_masterdata));

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
        }
    }
}