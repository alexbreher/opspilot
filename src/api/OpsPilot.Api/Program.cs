using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpsPilot.Api.Services;

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

var app = builder.Build();

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