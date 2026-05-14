using System.Text;
using System.Text.Json;
using OpsPilot.Contracts;
using RabbitMQ.Client;

namespace OpsPilot.Api.Services;

public class RabbitMqMessageBusPublisher : IMessageBusPublisher
{
    private readonly IConfiguration _config;
    private readonly IRabbitMqConnectionProvider _connProvider;
    private readonly OperationalEventService _ops;

    public RabbitMqMessageBusPublisher(IConfiguration config, IRabbitMqConnectionProvider connProvider, OperationalEventService ops)
    {
        _config = config;
        _connProvider = connProvider;
        _ops = ops;
    }

    public Task PublishIncidentCreatedAsync(IncidentCreatedMessage msg, CancellationToken ct)
    {
        if (!UseRabbitMq()) return Task.CompletedTask;

        TryPublishWithRetry(
            queueName: ToOptions().QueueIncidentCreated,
            message: msg,
            eventId: msg.EventId,
            correlationId: msg.CorrelationId);

        return Task.CompletedTask;
    }

    public Task PublishIncidentStatusChangedAsync(IncidentStatusChangedMessage msg, CancellationToken ct)
    {
        if (!UseRabbitMq()) return Task.CompletedTask;

        TryPublishWithRetry(
            queueName: ToOptions().QueueIncidentStatusChanged,
            message: msg,
            eventId: msg.EventId,
            correlationId: msg.CorrelationId);

        return Task.CompletedTask;
    }

    private void TryPublishWithRetry<T>(string queueName, T message, string eventId, string correlationId)
    {
        var retries = int.TryParse(_config["MessageBus:PublishRetryCount"], out var r) ? r : 3;
        var delayMs = int.TryParse(_config["MessageBus:PublishRetryDelayMs"], out var d) ? d : 250;
        var useConfirms = bool.TryParse(_config["MessageBus:UsePublisherConfirms"], out var c) && c;
        var confirmTimeoutMs = int.TryParse(_config["MessageBus:PublishConfirmTimeoutMs"], out var t) ? t : 2000;

        Exception? last = null;

        for (var attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                Publish(queueName, message, eventId, correlationId, useConfirms, confirmTimeoutMs);

                _ops.Add(new Domain.Entities.OperationalEvent
                {
                    Id = Guid.NewGuid(),
                    EventType = "RabbitPublishSucceeded",
                    Message = $"Published to {queueName} (EventId={eventId})",
                    CreatedBy = "opspilot-api",
                    CreatedAtUtc = DateTime.UtcNow,
                    CorrelationId = correlationId
                });

                return;
            }
            catch (Exception ex)
            {
                last = ex;

                _ops.Add(new Domain.Entities.OperationalEvent
                {
                    Id = Guid.NewGuid(),
                    EventType = "RabbitPublishFailed",
                    Message = $"Publish failed to {queueName} attempt {attempt}/{retries} (EventId={eventId}): {ex.Message}",
                    CreatedBy = "opspilot-api",
                    CreatedAtUtc = DateTime.UtcNow,
                    CorrelationId = correlationId
                });

                if (attempt < retries)
                    Thread.Sleep(delayMs);
            }
        }

        // IMPORTANT: Do NOT throw. We do not break API behavior.
        // We already logged failure as OperationalEvents.
        _ = last;
    }

    private void Publish<T>(string queueName, T message, string eventId, string correlationId, bool useConfirms, int confirmTimeoutMs)
    {
        using var channel = _connProvider.CreateChannel();

        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = channel.CreateBasicProperties();
        props.DeliveryMode = 2;
        props.MessageId = eventId;
        props.CorrelationId = correlationId;
        props.ContentType = "application/json";

        if (useConfirms)
        {
            channel.ConfirmSelect();
        }

        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: body);

        if (useConfirms)
        {
            // Wait for broker confirm; throws if not confirmed
            if (!channel.WaitForConfirms(TimeSpan.FromMilliseconds(confirmTimeoutMs)))
                throw new InvalidOperationException("Publish confirm timed out or was not acknowledged.");
        }
    }

    private bool UseRabbitMq() =>
        string.Equals(_config["MessageBus:Transport"], "RabbitMq", StringComparison.OrdinalIgnoreCase);

    private RabbitMqOptions ToOptions() => new()
    {
        Host = _config["MessageBus:RabbitMq:Host"] ?? "localhost",
        Username = _config["MessageBus:RabbitMq:Username"] ?? "guest",
        Password = _config["MessageBus:RabbitMq:Password"] ?? "guest"
    };
}