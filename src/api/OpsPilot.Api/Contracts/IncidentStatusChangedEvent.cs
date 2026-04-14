namespace OpsPilot.Api.Contracts;

public record IncidentStatusChangedEvent(
    Guid EventId,
    DateTime CreatedAtUtc,
    string CorrelationId,
    Guid IncidentId,
    string OldStatus,
    string NewStatus,
    string UpdatedBy
);