namespace OpsPilot.Worker;

public class OperationalEventIngestRequest
{
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string? SourceEventId { get; set; }

    public Guid? IncidentId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? TenantId { get; set; }
}