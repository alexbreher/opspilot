using System.Text;
using System.Text.Json;
using OpsPilot.Api.Contracts;
using RabbitMQ.Client;

namespace OpsPilot.Api.Services;

public class RabbitMqMessageBusPublisher : IMessageBusPublisher
{
    private readonly IConfiguration _config;

    public RabbitMqMessageBusPublisher(IConfiguration config)
    {
        _config = config;
    }

    public Task PublishIncidentCreatedAsync(IncidentCreatedMessage msg, CancellationToken ct)
    {
        if (!UseRabbitMq()) return Task.CompletedTask;
        Publish(ToOptions().QueueIncidentCreated, msg);
        return Task.CompletedTask;
    }

    public Task PublishIncidentStatusChangedAsync(IncidentStatusChangedMessage msg, CancellationToken ct)
    {
        if (!UseRabbitMq()) return Task.CompletedTask;
        Publish(ToOptions().QueueIncidentStatusChanged, msg);
        return Task.CompletedTask;
    }

    private void Publish<T>(string queueName, T message)
    {
        var opt = ToOptions();

        var factory = new ConnectionFactory
        {
            HostName = opt.Host,
            UserName = opt.Username,
            Password = opt.Password
        };

        using var conn = factory.CreateConnection();
        using var channel = conn.CreateModel();

        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = channel.CreateBasicProperties();
        props.DeliveryMode = 2; // persistent

        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: body);
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