using System;

namespace OpsPilot.Contracts;

public record IncidentCreatedMessage(
    string EventId,
    DateTime CreatedAtUtc,
    string CorrelationId,
    Guid IncidentId,
    Guid ServiceId,
    string Title,
    string Severity
);