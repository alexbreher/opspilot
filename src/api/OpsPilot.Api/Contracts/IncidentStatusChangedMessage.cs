namespace OpsPilot.Api.Contracts;

public record IncidentStatusChangedMessage(
    string EventId,
    DateTime CreatedAtUtc,
    string CorrelationId,
    Guid IncidentId,
    string OldStatus,
    string NewStatus,
    string UpdatedBy
);