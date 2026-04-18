using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpsPilot.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "http://localhost:5033";

        // ? THIS is where useV2/dequeuePath belongs (right at startup)
        var useV2 = bool.TryParse(_configuration["UseQueueV2"], out var b) && b;
        var dequeuePath = useV2 ? "/api/internal/event-queue/v2/dequeue" : "/api/internal/event-queue/dequeue";

        _logger.LogInformation("OpsPilot.Worker started. Polling {ApiBaseUrl} (UseQueueV2={UseQueueV2})",
            apiBaseUrl, useV2);

        var client = _httpClientFactory.CreateClient("OpsPilotApi");
        client.BaseAddress = new Uri(apiBaseUrl);

        // Internal API key header (optional)
        var internalKey = _configuration["InternalApiKey"];
        if (!string.IsNullOrWhiteSpace(internalKey))
        {
            client.DefaultRequestHeaders.Remove("X-Internal-Key");
            client.DefaultRequestHeaders.Add("X-Internal-Key", internalKey);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // ? Dequeue from the selected path
                var response = await client.PostAsync(dequeuePath, content: null, stoppingToken);

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                response.EnsureSuccessStatusCode();

                string? eventType;
                string? eventId;
                JsonElement payload;

                if (useV2)
                {
                    // ? V2 envelope includes EventId explicitly
                    var env = await response.Content.ReadFromJsonAsync<EventEnvelopeV2>(cancellationToken: stoppingToken);

                    if (env == null || string.IsNullOrWhiteSpace(env.Type) || string.IsNullOrWhiteSpace(env.EventId) ||
                        env.Payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                    {
                        _logger.LogWarning("Invalid v2 envelope dequeued.");
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        continue;
                    }

                    eventType = env.Type;
                    eventId = env.EventId;
                    payload = env.Payload;
                }
                else
                {
                    // ? V1 envelope returned { type, payload } where payload may or may not contain eventId
                    var env = await response.Content.ReadFromJsonAsync<EventEnvelopeV1>(cancellationToken: stoppingToken);

                    if (env == null || string.IsNullOrWhiteSpace(env.Type) ||
                        env.Payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                    {
                        _logger.LogWarning("Invalid v1 envelope dequeued.");
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        continue;
                    }

                    eventType = env.Type;
                    payload = env.Payload;

                    // best-effort extraction (camelCase/PascalCase)
                    eventId =
                        payload.TryGetProperty("eventId", out var id1) ? id1.GetString() :
                        payload.TryGetProperty("EventId", out var id2) ? id2.GetString() :
                        null;

                    if (string.IsNullOrWhiteSpace(eventId))
                    {
                        _logger.LogWarning("Dequeued event has no EventId. Type={Type}", eventType);
                        // With v1, we cannot guarantee idempotency. Skip.
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        continue;
                    }
                }

                // 1) Check already processed (API store)
                var alreadyProcessed = await CheckProcessedAsync(client, eventId!, stoppingToken);
                if (alreadyProcessed)
                {
                    _logger.LogWarning("Skipping already processed event. EventId={EventId} Type={Type}", eventId, eventType);
                    continue;
                }

                _logger.LogInformation("Processing event Type={Type} EventId={EventId}", eventType, eventId);

                // 2) Post operational event back (observability)
                await PostOperationalEventWithRetryAsync(
                    client,
                    new OperationalEventIngestRequest
                    {
                        EventType = "EventProcessed",
                        Message = $"Worker processed {eventType}",
                        CreatedBy = "opspilot-worker",
                        CorrelationId = ExtractCorrelationId(payload),
                        SourceEventId = eventId,
                        IncidentId = ExtractGuid(payload, "IncidentId"),
                        ServiceId = ExtractGuid(payload, "ServiceId")
                    },
                    stoppingToken);

                // 3) Mark processed
                await MarkProcessedAsync(client, eventId!, eventType!, stoppingToken);

                _logger.LogInformation("Marked processed. EventId={EventId}", eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker poll/process failed");
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }

    private static string ExtractCorrelationId(JsonElement payload)
    {
        var corr =
            payload.TryGetProperty("correlationId", out var c1) ? c1.GetString() :
            payload.TryGetProperty("CorrelationId", out var c2) ? c2.GetString() :
            null;

        return string.IsNullOrWhiteSpace(corr) ? "n/a" : corr!;
    }

    private static Guid? ExtractGuid(JsonElement payload, string propertyName)
    {
        if (payload.TryGetProperty(propertyName, out var p) && p.ValueKind == JsonValueKind.String)
        {
            var s = p.GetString();
            if (Guid.TryParse(s, out var g)) return g;
        }
        return null;
    }

    private async Task<bool> CheckProcessedAsync(HttpClient client, string eventId, CancellationToken ct)
    {
        var resp = await client.PostAsJsonAsync("/api/internal/processed-events/check", new { eventId }, ct);
        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadFromJsonAsync<ProcessedCheckResponse>(cancellationToken: ct);
        return body?.AlreadyProcessed ?? false;
    }

    private async Task MarkProcessedAsync(HttpClient client, string eventId, string eventType, CancellationToken ct)
    {
        var resp = await client.PostAsJsonAsync("/api/internal/processed-events/mark", new
        {
            eventId,
            eventType,
            processedAtUtc = DateTime.UtcNow
        }, ct);

        resp.EnsureSuccessStatusCode();
    }

    private async Task PostOperationalEventWithRetryAsync(HttpClient client, OperationalEventIngestRequest request, CancellationToken ct)
    {
        const int maxAttempts = 5;
        var delay = TimeSpan.FromSeconds(1);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var resp = await client.PostAsJsonAsync("/api/internal/operational-events", request, ct);
                if (resp.IsSuccessStatusCode)
                    return;

                _logger.LogWarning("Failed to ingest operational event. Status={Status}. Attempt {Attempt}/{Max}.",
                    resp.StatusCode, attempt, maxAttempts);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error ingesting operational event. Attempt {Attempt}/{Max}.", attempt, maxAttempts);
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 10));
            }
        }

        _logger.LogError("Operational event ingestion failed after retries. SourceEventId={SourceEventId}", request.SourceEventId);
    }

    // V1 response envelope from /api/internal/event-queue/dequeue
    private sealed class EventEnvelopeV1
    {
        public string? Type { get; set; }
        public JsonElement Payload { get; set; }
    }

    // ? V2 envelope from /api/internal/event-queue/v2/dequeue
    // This is the class you asked about: it only exists to deserialize v2 responses.
    private sealed class EventEnvelopeV2
    {
        public string? Type { get; set; }
        public string? EventId { get; set; }
        public JsonElement Payload { get; set; }
    }

    private sealed class ProcessedCheckResponse
    {
        public string EventId { get; set; } = string.Empty;
        public bool AlreadyProcessed { get; set; }
    }
}