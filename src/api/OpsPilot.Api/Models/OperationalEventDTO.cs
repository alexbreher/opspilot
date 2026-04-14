public class OperationalEventDto
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? IncidentId { get; set; }

    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    public string CorrelationId { get; set; } = string.Empty;
}