using OpsPilot.Api.Domain.Entities;

namespace OpsPilot.Api.Services;

public class InMemoryIncidentTimeLineStore : IIncidentTimeLineStore
{
    private readonly List<IncidentTimelineEvent> _events = new()
    {
        new IncidentTimelineEvent
        {
            Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
            IncidentId = Guid.Parse("40000000-0000-0000-0000-000000000001"),
            EventType = "Created",
            Message = "Incident was created and triaged by on-call engineer.",
            CreatedBy = "system",
            CreatedAtUtc = DateTime.UtcNow.AddHours(-4)
        },
        new IncidentTimelineEvent
        {
            Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
            IncidentId = Guid.Parse("40000000-0000-0000-0000-000000000001"),
            EventType = "Investigation",
            Message = "Initial analysis indicates authentication dependency timeout.",
            CreatedBy = "alex@acme.com",
            CreatedAtUtc = DateTime.UtcNow.AddHours(-3)
        }
    };

    public IReadOnlyList<IncidentTimelineEvent> GetAll() => _events;

    public void Add(IncidentTimelineEvent timelineEvent)
    {
        _events.Add(timelineEvent);
    }

}