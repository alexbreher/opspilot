using OpsPilot.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient("OpsPilotApi");
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<RabbitMqConsumerHostedService>();

var hostApp = builder.Build();
hostApp.Run();