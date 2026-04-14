namespace OpsPilot.Api.Domain.Entities;

public class OperationalEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Optional scoping fields
    public Guid? TenantId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? IncidentId { get; set; }

    public string EventType { get; set; } = string.Empty;     // e.g., RunbookCreated, Deployment, ConfigChanged
    public string Message { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // For request tracing / log correlation
    public string CorrelationId { get; set; } = string.Empty;
}