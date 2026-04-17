using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpsPilot.Api.Services;
using OpsPilot.Api.Middleware;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpsPilot.Api.Messaging;

var builder = WebApplication.CreateBuilder(args);

//Structured logging is provided by default
builder.Services.AddHttpLogging(_ => { });

//Health Checks
builder.Services.AddHealthChecks();

//Controllers = Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TenantAccessService>();
builder.Services.AddSingleton<ITenantAccessService>(sp => sp.GetRequiredService<TenantAccessService>());
builder.Services.AddSingleton<ServiceCatalogService>();
builder.Services.AddSingleton<IncidentService>();
builder.Services.AddSingleton<IncidentTimelineService>();
builder.Services.AddSingleton<IIncidentTimeLineStore, InMemoryIncidentTimeLineStore>();
builder.Services.AddSingleton<IOperationalEventStore, InMemoryOperationalEventStore>();
builder.Services.AddSingleton<OperationalEventService>();
builder.Services.AddSingleton<PostmortemService>();
builder.Services.AddSingleton<RunbookService>();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: "OpsPilot.Api"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter(); // traces only
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
        // NOTE: no ConsoleExporter here, so no repeated console spam
    });
builder.Services.AddSingleton<IEventQueue, InMemoryEventQueue>();
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
builder.Services.AddSingleton<IProcessedEventStore, InMemoryProcessedEventStore>();


var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<InternalApiKeyMiddleware>();

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new {name = e.Key, status = e.Value.Status.ToString() })
        };
        await context.Response.WriteAsJsonAsync(payload);
    }
});

app.Run();