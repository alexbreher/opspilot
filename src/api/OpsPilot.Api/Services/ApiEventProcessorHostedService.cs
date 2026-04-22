using Microsoft.Extensions.Hosting;
using OpsPilot.Api.Domain.Entities;
using OpsPilot.Api.Messaging;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Services;

public class ApiEventProcessorHostedService : BackgroundService
{
    private readonly ILogger<ApiEventProcessorHostedService> _logger;
    private readonly IBackgroundEventQueue _queue;
    private readonly IProcessedEventStore _processedEventStore;
    private readonly OperationalEventService _ops;

    public ApiEventProcessorHostedService(ILogger<ApiEventProcessorHostedService> logger, IBackgroundEventQueue queue, IProcessedEventStore processedEventStore, OperationalEventService ops)
    {
        _logger = logger;
        _queue = queue;
        _processedEventStore = processedEventStore;
        _ops = ops;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ApiEventProcessorHosterService Started.");

        await foreach (var env in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                if (env == null || string.IsNullOrWhiteSpace(env.EventId) || string.IsNullOrWhiteSpace(env.Type))
                {
                    _logger.LogWarning("Skipped invalid envelope.");
                    continue;
                }

                if (_processedEventStore.Exists(env.EventId))
                {
                    _logger.LogInformation("Skipping already processed event. EventId={EventId} Type={Type}", env.EventId, env.Type);
                    continue;
                }
                // Record that we processed it (OperationalEvents)
                _ops.Add(new OperationalEvent
                {
                    Id = Guid.NewGuid(),
                    EventType = "EventProcessedByApi",
                    Message = $"API processor handled {env.Type}",
                    CreatedBy = "opspilot-api-processor",
                    CreatedAtUtc = DateTime.UtcNow,
                    CorrelationId = string.IsNullOrWhiteSpace(env.Payload.GetPropertyOrDefault("correlationId"))
                        ? "n/a"
                        : env.Payload.GetPropertyOrDefault("correlationId")!
                });

                // Mark processed (idempotency)
                _processedEventStore.Mark(env.EventId, env.Type, DateTime.UtcNow);

                _logger.LogInformation("Processed and marked. EventId={EventId} Type={Type}", env.EventId, env.Type);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API processor failed processing EventId={EventId} Type={Type}", env?.EventId, env?.Type);
            }
        }
    }
}
// Small helper for JsonElement safe read
internal static class JsonElementExtensions
{
    public static string? GetPropertyOrDefault(this System.Text.Json.JsonElement element, string propertyName)
    {
        if (element.ValueKind != System.Text.Json.JsonValueKind.Object) return null;
        if (element.TryGetProperty(propertyName, out var p) && p.ValueKind == System.Text.Json.JsonValueKind.String)
            return p.GetString();
        return null;
    }
}