namespace OpsPilot.Api.Contracts;

public record IncidentCreatedEvent(
    Guid EventId,
    DateTime CreatedAtUtc,
    string CorrelationId,
    Guid IncidentId,
    Guid ServiceId,
    string Title,
    string Severity
);