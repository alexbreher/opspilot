using OpsPilot.Api.Contracts;

namespace OpsPilot.Api.Services;

public interface IMessageBusPublisher
{
    Task PublishIncidentCreatedAsync(IncidentCreatedMessage msg, CancellationToken ct);
    Task PublishIncidentStatusChangedAsync(IncidentStatusChangedMessage msg, CancellationToken ct);
}