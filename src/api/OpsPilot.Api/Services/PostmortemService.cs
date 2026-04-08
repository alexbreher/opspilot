using OpsPilot.Api.Domain.Entities;
using OpsPilot.Api.Models;

namespace OpsPilot.Api.Services;

public class PostmortemService
{
    private readonly List<Postmortem> _postmortems = new();
    private readonly List<ActionItem> _actionItems = new();

    private readonly IncidentService _incidentService;
    private readonly IIncidentTimeLineStore _timelineStore;

    public PostmortemService(IncidentService incidentService, IIncidentTimeLineStore timelineStore)
    {
        _incidentService = incidentService;
        _timelineStore = timelineStore;
    }

    public PostmortemDTO? GetByIncidentId(Guid incidentId)
    {
        var pm = _postmortems.FirstOrDefault(x => x.IncidentId == incidentId);
        if (pm == null) return null;

        return Map(pm);
    }

    public PostmortemDTO CreateForIncident(Guid incidentId, CreatePostmortemRequest request)
    {
        var incident = _incidentService.GetEntityById(incidentId);
        if (incident == null)
        {
            throw new InvalidOperationException("Incident not found.");
        }

        // Enforced rule
        if (!string.Equals(incident.Status, "Resolved", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Postmortem can only be created when incident status is Resolved.");
        }

        // Prevent duplicates (one postmortem per incident)
        if (_postmortems.Any(x => x.IncidentId == incidentId))
        {
            throw new InvalidOperationException("Postmortem already exists for this incident.");
        }

        var pm = new Postmortem
        {
            Id = Guid.NewGuid(),
            IncidentId = incidentId,
            Summary = request.Summary.Trim(),
            RootCause = request.RootCause.Trim(),
            Impact = request.Impact.Trim(),
            Resolution = request.Resolution.Trim(),
            LessonsLearned = request.LessonsLearned.Trim(),
            CreatedBy = request.CreatedBy.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _postmortems.Add(pm);

        // Timeline event
        _timelineStore.Add(new IncidentTimelineEvent
        {
            Id = Guid.NewGuid(),
            IncidentId = incidentId,
            EventType = "PostmortemCreated",
            Message = $"Postmortem created by {pm.CreatedBy}.",
            CreatedBy = pm.CreatedBy,
            CreatedAtUtc = DateTime.UtcNow
        });

        return Map(pm);
    }

    public ActionItemDto AddActionItem(Guid postmortemId, CreateActionItemRequest request)
    {
        var pm = _postmortems.FirstOrDefault(x => x.Id == postmortemId);
        if (pm == null)
        {
            throw new InvalidOperationException("Postmortem not found.");
        }

        var item = new ActionItem
        {
            Id = Guid.NewGuid(),
            PostmortemId = postmortemId,
            Title = request.Title.Trim(),
            Owner = request.Owner.Trim(),
            Status = "Open",
            DueDateUtc = request.DueDateUtc.Kind == DateTimeKind.Utc ? request.DueDateUtc : request.DueDateUtc.ToUniversalTime(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _actionItems.Add(item);

        // Timeline event on the incident
        _timelineStore.Add(new IncidentTimelineEvent
        {
            Id = Guid.NewGuid(),
            IncidentId = pm.IncidentId,
            EventType = "ActionItemAdded",
            Message = $"Action item added: '{item.Title}' (Owner: {item.Owner}, Due: {item.DueDateUtc:yyyy-MM-dd}).",
            CreatedBy = pm.CreatedBy,
            CreatedAtUtc = DateTime.UtcNow
        });

        return new ActionItemDto
        {
            Id = item.Id,
            PostmortemId = item.PostmortemId,
            Title = item.Title,
            Owner = item.Owner,
            Status = item.Status,
            DueDateUtc = item.DueDateUtc,
            CreatedAtUtc = item.CreatedAtUtc
        };
    }

    private static PostmortemDTO Map(Postmortem pm) =>
        new PostmortemDTO
        {
            Id = pm.Id,
            IncidentId = pm.IncidentId,
            Summary = pm.Summary,
            RootCause = pm.RootCause,
            Impact = pm.Impact,
            Resolution = pm.Resolution,
            LessonsLearned = pm.LessonsLearned,
            CreatedBy = pm.CreatedBy,
            CreatedAtUtc = pm.CreatedAtUtc
        };
}