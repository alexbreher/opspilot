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

    // basic idempotency: remember processed event ids during this run
    private readonly HashSet<string> _processed = new(StringComparer.OrdinalIgnoreCase);

    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "http://localhost:5033";
        _logger.LogInformation("OpsPilot.Worker started. Polling {ApiBaseUrl}", apiBaseUrl);

        var client = _httpClientFactory.CreateClient("OpsPilotApi");
        client.BaseAddress = new Uri(apiBaseUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1) Dequeue one event from API queue
                var response = await client.PostAsync("/api/internal/event-queue/dequeue", content: null, stoppingToken);

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var envelope = await response.Content.ReadFromJsonAsync<EventEnvelope>(cancellationToken: stoppingToken);

                if (envelope == null || string.IsNullOrWhiteSpace(envelope.Type) ||
                    envelope.Payload.ValueKind == JsonValueKind.Undefined ||
                    envelope.Payload.ValueKind == JsonValueKind.Null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                // 2) Best-effort eventId extraction (handles camelCase/PascalCase)
                var eventId =
                    envelope.Payload.TryGetProperty("eventId", out var id1) ? id1.GetString() :
                    envelope.Payload.TryGetProperty("EventId", out var id2) ? id2.GetString() :
                    null;

                if (!string.IsNullOrWhiteSpace(eventId) && _processed.Contains(eventId!))
                {
                    _logger.LogWarning("Duplicate event ignored. EventId={EventId}", eventId);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(eventId))
                    _processed.Add(eventId!);

                _logger.LogInformation("Processed event Type={Type} EventId={EventId}", envelope.Type, eventId ?? "(none)");

                // 3) Post an OperationalEvent back to API (observability)
                await PostOperationalEventWithRetryAsync(
                    client,
                    new OperationalEventIngestRequest
                    {
                        EventType = "EventProcessed",
                        Message = $"Worker processed {envelope.Type}",
                        CreatedBy = "opspilot-worker",
                        CorrelationId = ExtractCorrelationId(envelope.Payload),
                        SourceEventId = eventId,
                        IncidentId = ExtractGuid(envelope.Payload, "IncidentId"),
                        ServiceId = ExtractGuid(envelope.Payload, "ServiceId")
                    },
                    stoppingToken);
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
                {
                    return;
                }

                // Non-success status: retry a few times
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
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 10)); // cap backoff
            }
        }

        // Dead-letter: write to disk so it’s not lost
        await WriteDeadLetterAsync(request, ct);
    }

    private async Task WriteDeadLetterAsync(OperationalEventIngestRequest request, CancellationToken ct)
    {
        try
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "deadletters");
            Directory.CreateDirectory(dir);

            var file = Path.Combine(dir, $"op-event-deadletter-{DateTime.UtcNow:yyyyMMddHHmmssfff}.json");
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(file, json, ct);
            _logger.LogError("Operational event dead-lettered to {File}", file);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write dead-letter file");
        }
    }

    private sealed class EventEnvelope
    {
        public string? Type { get; set; }
        public JsonElement Payload { get; set; }
    }
}