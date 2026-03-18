using OpsPilot.Api.Domain.Entities;
using OpsPilot.Api.Models;

namespace OpsPilot.Api.Services;

public class IncidentTimelineService
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

    private readonly IncidentService _incidentService;

    public IncidentTimelineService(IncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    public IEnumerable<IncidentTimelineEventDto> GetByIncidentId(Guid incidentId)
    {
        return _events.Where(e => e.IncidentId == incidentId).OrderBy(e => e.CreatedAtUtc)
                      .Select(e => new IncidentTimelineEventDto
                      {
                          Id = e.Id,
                          IncidentId = e.IncidentId,
                          EventType = e.EventType,
                          Message = e.Message,
                          CreatedBy = e.CreatedBy,
                          CreatedAtUtc = e.CreatedAtUtc
                      });
    }

    public  IncidentTimelineEventDto Create (Guid incidentId, CreateIncidentTimelineEventRequest request)
    {
        if (!_incidentService.Exists(incidentId))
        {
            throw new InvalidOperationException("Incident not found");
        }
        var timelineEvent = new IncidentTimelineEvent
        {
            Id = Guid.NewGuid(),
            IncidentId = incidentId,
            EventType = request.EventType.Trim(),
            Message = request.Message.Trim(),
            CreatedBy = request.CreatedBy.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _events.Add(timelineEvent);

        return new IncidentTimelineEventDto
        {
            Id = timelineEvent.Id,
            IncidentId = timelineEvent.IncidentId,
            EventType = timelineEvent.EventType,
            Message = timelineEvent.Message,
            CreatedBy = timelineEvent.CreatedBy,
            CreatedAtUtc = timelineEvent.CreatedAtUtc
        };
    }

}