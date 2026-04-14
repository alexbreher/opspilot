using System.Diagnostics;

namespace OpsPilot.Api.Middleware;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        // Push into response header so clients can see it
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // Add to logging scope for all logs in this request
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            // Also put it in Activity for tracing
            Activity.Current?.SetTag("correaltion_id", correlationId);
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Response.Headers.TryGetValue(HeaderName, out var values))
        {
            var existing = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(existing))
                return existing!;
        }
        return Guid.NewGuid().ToString("N");
    }


}