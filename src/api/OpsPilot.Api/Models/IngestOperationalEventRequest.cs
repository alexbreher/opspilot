namespace OpsPilot.Api.Models;

public class IngestOperationalEventRequest
{
    public Guid? TenantId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? IncidentId { get; set; }

    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;

    // optional: original event id for traceability
    public string? SourceEventId { get; set; }
}