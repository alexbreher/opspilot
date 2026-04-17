namespace OpsPilot.Api.Middleware;

public class InternalApiKeyMiddleware
{
    public const string HeaderName = "X-Internal-Key";

    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public InternalApiKeyMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task Invoke(HttpContext context)
    {
        // Only protect internal endpoints
        if (!context.Request.Path.StartsWithSegments("/api/internal", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // If not configured, do nothing (keeps behavior exactly as today)
        var expectedKey = _config["InternalApiKey"];
        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var provided) ||
            string.IsNullOrWhiteSpace(provided) ||
            !string.Equals(provided.ToString(), expectedKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized (missing/invalid internal API key).");
            return;
        }

        await _next(context);
    }
}