using OpsPilot.Api.Domain.Entities;

namespace OpsPilot.Api.Services;

public interface IIncidentTimeLineStore
{
    IReadOnlyList<IncidentTimelineEvent> GetAll();
    void Add(IncidentTimelineEvent timelineEvent);
}