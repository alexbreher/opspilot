using RabbitMQ.Client;

namespace OpsPilot.Api.Services;

public interface IRabbitMqConnectionProvider
{
    IModel CreateChannel();
}

public class RabbitMqConnectionProvider : IRabbitMqConnectionProvider, IDisposable
{
    private readonly IConfiguration _config;
    private readonly object _lock = new();
    private IConnection? _connection;

    public RabbitMqConnectionProvider(IConfiguration config)
    {
        _config = config;
    }

    public IModel CreateChannel()
    {
        var conn = GetOrCreateConnection();
        return conn.CreateModel(); // channel per publish (channels are not thread-safe)
    }

    private IConnection GetOrCreateConnection()
    {
        lock (_lock)
        {
            if (_connection != null && _connection.IsOpen)
                return _connection;

            var host = _config["MessageBus:RabbitMq:Host"] ?? "localhost";
            var user = _config["MessageBus:RabbitMq:Username"] ?? "guest";
            var pass = _config["MessageBus:RabbitMq:Password"] ?? "guest";

            var factory = new ConnectionFactory
            {
                HostName = host,
                UserName = user,
                Password = pass,
                AutomaticRecoveryEnabled = true,   // safe
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };

            _connection = factory.CreateConnection("opspilot-api");
            return _connection;
        }
    }

    public void Dispose()
    {
        try { _connection?.Dispose(); } catch { }
    }
}