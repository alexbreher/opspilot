using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OpsPilot.Api.Contracts;

namespace OpsPilot.Worker;

public class RabbitMqConsumerHostedService : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerHostedService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqConsumerHostedService(
        ILogger<RabbitMqConsumerHostedService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!UseRabbitMq())
        {
            _logger.LogInformation("RabbitMq consumer disabled (MessageBus:Transport != RabbitMq).");
            return;
        }

        var opt = GetOptions();
        var api = CreateApiClient();

        var factory = new ConnectionFactory
        {
            HostName = opt.Host,
            UserName = opt.Username,
            Password = opt.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        DeclareQueue(opt.QueueIncidentCreated);
        DeclareQueue(opt.QueueIncidentStatusChanged);

        _channel.BasicQos(0, 1, false);
        ConsumeQueue<OpsPilot.Api.Contracts.IncidentCreatedMessage>(api, opt.QueueIncidentCreated, HandleIncidentCreatedAsync, stoppingToken);
        ConsumeQueue<OpsPilot.Api.Contracts.IncidentStatusChangedMessage>(api, opt.QueueIncidentStatusChanged, HandleIncidentStatusChangedAsync, stoppingToken);

        _logger.LogInformation("RabbitMq consumer running.");
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    private void ConsumeQueue<T>(
        HttpClient api,
        string queueName,
        Func<HttpClient, T, CancellationToken, Task> handler,
        CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<T>(json);

                if (msg == null)
                {
                    _logger.LogWarning("Null message deserialized from {Queue}", queueName);
                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                await handler(api, msg, stoppingToken);

                _channel!.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMq processing failed for {Queue}. Requeueing.", queueName);
                _channel!.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel!.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        _logger.LogInformation("Consuming queue {Queue}", queueName);
    }

    private async Task HandleIncidentCreatedAsync(HttpClient api, IncidentCreatedMessage msg, CancellationToken ct)
    {
        if (await IsAlreadyProcessed(api, msg.EventId, ct))
        {
            _logger.LogInformation("Skip already processed IncidentCreated EventId={EventId}", msg.EventId);
            return;
        }

        await IngestOperational(api, "EventProcessed", $"RabbitMQ consumed IncidentCreated ({msg.IncidentId})", msg.CorrelationId, msg.EventId, msg.IncidentId, msg.ServiceId, ct);
        await MarkProcessed(api, msg.EventId, "IncidentCreatedMessage", ct);

        _logger.LogInformation("Processed IncidentCreated EventId={EventId}", msg.EventId);
    }

    private async Task HandleIncidentStatusChangedAsync(HttpClient api, IncidentStatusChangedMessage msg, CancellationToken ct)
    {
        if (await IsAlreadyProcessed(api, msg.EventId, ct))
        {
            _logger.LogInformation("Skip already processed StatusChanged EventId={EventId}", msg.EventId);
            return;
        }

        await IngestOperational(api, "EventProcessed", $"RabbitMQ consumed IncidentStatusChanged ({msg.IncidentId})", msg.CorrelationId, msg.EventId, msg.IncidentId, null, ct);
        await MarkProcessed(api, msg.EventId, "IncidentStatusChangedMessage", ct);

        _logger.LogInformation("Processed StatusChanged EventId={EventId}", msg.EventId);
    }

    private HttpClient CreateApiClient()
    {
        var apiBaseUrl = _config["ApiBaseUrl"] ?? "http://localhost:5033";
        var client = _httpClientFactory.CreateClient("OpsPilotApi");
        client.BaseAddress = new Uri(apiBaseUrl);

        var internalKey = _config["InternalApiKey"];
        if (!string.IsNullOrWhiteSpace(internalKey))
        {
            client.DefaultRequestHeaders.Remove("X-Internal-Key");
            client.DefaultRequestHeaders.Add("X-Internal-Key", internalKey);
        }

        return client;
    }

    private async Task<bool> IsAlreadyProcessed(HttpClient api, string eventId, CancellationToken ct)
    {
        var resp = await api.PostAsJsonAsync("/api/internal/processed-events/check", new { eventId }, ct);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<ProcessedCheckResponse>(cancellationToken: ct);
        return body?.AlreadyProcessed ?? false;
    }

    private async Task MarkProcessed(HttpClient api, string eventId, string eventType, CancellationToken ct)
    {
        var resp = await api.PostAsJsonAsync("/api/internal/processed-events/mark", new
        {
            eventId,
            eventType,
            processedAtUtc = DateTime.UtcNow
        }, ct);

        resp.EnsureSuccessStatusCode();
    }

    private async Task IngestOperational(HttpClient api, string eventType, string message, string correlationId, string sourceEventId, Guid incidentId, Guid? serviceId, CancellationToken ct)
    {
        var resp = await api.PostAsJsonAsync("/api/internal/operational-events", new
        {
            eventType,
            message,
            createdBy = "opspilot-rabbitmq-worker",
            correlationId,
            sourceEventId,
            incidentId,
            serviceId
        }, ct);

        resp.EnsureSuccessStatusCode();
    }

    private void DeclareQueue(string queueName)
    {
        _channel!.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    private bool UseRabbitMq() =>
        string.Equals(_config["MessageBus:Transport"], "RabbitMq", StringComparison.OrdinalIgnoreCase);

    private RabbitMqOptions GetOptions() => new()
    {
        Host = _config["MessageBus:RabbitMq:Host"] ?? "localhost",
        Username = _config["MessageBus:RabbitMq:Username"] ?? "guest",
        Password = _config["MessageBus:RabbitMq:Password"] ?? "guest"
    };

    private sealed class ProcessedCheckResponse
    {
        public string EventId { get; set; } = string.Empty;
        public bool AlreadyProcessed { get; set; }
    }

    public override void Dispose()
    {
        try { _channel?.Dispose(); } catch { }
        try { _connection?.Dispose(); } catch { }
        base.Dispose();
    }
}