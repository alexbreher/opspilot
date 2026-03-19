using OpsPilot.Api.Domain.Entities;
using OpsPilot.Api.Models;

namespace OpsPilot.Api.Services;

public class IncidentTimelineService
{
    private readonly IIncidentTimeLineStore _store;

    public IncidentTimelineService(IIncidentTimeLineStore store)
    {
        _store = store;
    }

    public IEnumerable<IncidentTimelineEventDto> GetByIncidentId(Guid incidentId)
    {
        return _store.GetAll()
            .Where(e => e.IncidentId == incidentId)
            .OrderBy(e => e.CreatedAtUtc)
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

    public IncidentTimelineEventDto Create(Guid incidentId, CreateIncidentTimelineEventRequest request)
    {
        var timelineEvent = new IncidentTimelineEvent
        {
            Id = Guid.NewGuid(),
            IncidentId = incidentId,
            EventType = request.EventType.Trim(),
            Message = request.Message.Trim(),
            CreatedBy = request.CreatedBy.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _store.Add(timelineEvent);

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