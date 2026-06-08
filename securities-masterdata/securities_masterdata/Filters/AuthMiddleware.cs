using ttm_system.Shared.Attributes;

namespace securities_masterdata.Filters;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var preLoadAttribute = endpoint.Metadata.Where(meta => meta.GetType() == typeof(TTMAuthAttribute))
            .Select(meta => (TTMAuthAttribute)meta).FirstOrDefault();

        if (preLoadAttribute == null)
        {
            await _next(context);
            return;
        }

        //TODO: get user data and claims
        //TODO: compare to the level of what the API endpoint requires

        await _next(context);
    }
}