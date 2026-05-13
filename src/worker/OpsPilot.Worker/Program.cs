using OpsPilot.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient("OpsPilotApi");

// Decide which worker mode runs
var transport = builder.Configuration["MessageBus:Transport"] ?? "Channel";

if (string.Equals(transport, "RabbitMq", StringComparison.OrdinalIgnoreCase))
{
    // RabbitMQ consumer mode
    builder.Services.AddHostedService<RabbitMqConsumerHostedService>();
}
else
{
    // Legacy HTTP polling mode (v1/v2 internal queue)
    builder.Services.AddHostedService<Worker>();
}

var host = builder.Build();
host.Run();