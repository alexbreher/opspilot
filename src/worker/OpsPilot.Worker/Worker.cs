using System.Net.Http.Json;
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
                var response = await client.PostAsync("/api/internal/event-queue/dequeue", content: null, stoppingToken);

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var envelope = await response.Content.ReadFromJsonAsync<EventEnvelope>(cancellationToken: stoppingToken);

                if (envelope == null || string.IsNullOrWhiteSpace(envelope.Type) ||
                    envelope.Payload.ValueKind == System.Text.Json.JsonValueKind.Undefined ||
                    envelope.Payload.ValueKind == System.Text.Json.JsonValueKind.Null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                // best-effort idempotency: use EventId if present
                var eventId = envelope.Payload.TryGetProperty("eventId", out var idProp) ? idProp.GetString() : null;
                if (!string.IsNullOrWhiteSpace(eventId) && _processed.Contains(eventId!))
                {
                    _logger.LogWarning("Duplicate event ignored. EventId={EventId}", eventId);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(eventId))
                    _processed.Add(eventId!);

                _logger.LogInformation("Processed event Type={Type} EventId={EventId}", envelope.Type, eventId ?? "(none)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker poll/process failed");
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }

    private sealed class EventEnvelope
    {
        public string? Type { get; set; }
        public System.Text.Json.JsonElement Payload { get; set; }
    }
}
