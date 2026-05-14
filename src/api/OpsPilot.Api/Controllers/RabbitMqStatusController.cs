using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RabbitMqStatusController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IRabbitMqConnectionProvider _connProvider;

    public RabbitMqStatusController(IConfiguration config, IRabbitMqConnectionProvider connProvider)
    {
        _config = config;
        _connProvider = connProvider;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var transport = _config["MessageBus:Transport"] ?? "Channel";

        if (!string.Equals(transport, "RabbitMq", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new { transport, enabled = false });
        }

        try
        {
            using var channel = _connProvider.CreateChannel();

            // passive declare reads counts without creating (will throw if missing)
            var createdQueue = channel.QueueDeclarePassive("opspilot.incident.created");
            var statusQueue = channel.QueueDeclarePassive("opspilot.incident.status_changed");

            return Ok(new
            {
                transport,
                enabled = true,
                host = _config["MessageBus:RabbitMq:Host"],
                queues = new[]
                {
                    new { name = "opspilot.incident.created", messages = createdQueue.MessageCount, consumers = createdQueue.ConsumerCount },
                    new { name = "opspilot.incident.status_changed", messages = statusQueue.MessageCount, consumers = statusQueue.ConsumerCount }
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                transport,
                enabled = true,
                host = _config["MessageBus:RabbitMq:Host"],
                status = "unhealthy",
                error = ex.Message
            });
        }
    }
}