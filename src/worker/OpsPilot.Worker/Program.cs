using OpsPilot.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient("OpsPilotApi");
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
