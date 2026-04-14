using OpsPilot.Api.Domain.Entities;

namespace OpsPilot.Api.Services;

public class InMemoryOperationalEventStore : IOperationalEventStore
{
    private readonly List<OperationalEvent> _events = new()
    {
        new OperationalEvent
        {
            Id = Guid.Parse("70000000-0000-0000-0000-000000000001"),
            EventType = "SystemStarted",
            Message = "OpsPilot API started.",
            CreatedBy = "system",
            CreatedAtUtc = DateTime.UtcNow,
            CorrelationId = "bootstrap"
        }
    };

    public IReadOnlyList<OperationalEvent> GetAll() => _events;
    public void Add(OperationalEvent operationalEvent) => _events.Add(operationalEvent);
}