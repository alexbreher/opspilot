using OpsPilot.Api.Domain.Entities;
using OpsPilot.Api.Models;

namespace OpsPilot.Api.Services;

public class OperationalEventService
{
    private IOperationalEventStore _store;

    public OperationalEventService(IOperationalEventStore store)
    {
        _store = store;
    }
    public IEnumerable<OperationalEventDto> GetLatest(int limit = 50)
    {
        limit = Math.Clamp(limit, 1, 200);

        return _store.GetAll()
            .OrderByDescending(e => e.CreatedAtUtc)
            .Take(limit)
            .Select(e => new OperationalEventDto
            {
                Id = e.Id,
                TenantId = e.TenantId,
                ServiceId = e.ServiceId,
                IncidentId = e.IncidentId,
                EventType = e.EventType,
                Message = e.Message,
                CreatedBy = e.CreatedBy,
                CreatedAtUtc = e.CreatedAtUtc,
                CorrelationId = e.CorrelationId
            });
    }

    public IEnumerable<OperationalEventDto> GetByService(Guid serviceId, int limit = 50)
    {
        limit = Math.Clamp(limit,1,200);

        return _store.GetAll().Where(e => e.ServiceId == serviceId).OrderByDescending(e => e.CreatedAtUtc).Take(limit).Select(Map);
    }

    public void Add(OperationalEvent e) => _store.Add(e);

    private static OperationalEventDto Map(OperationalEvent e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ServiceId = e.ServiceId,
        IncidentId = e.IncidentId,
        EventType = e.EventType,
        Message = e.Message,
        CreatedBy = e.CreatedBy,
        CreatedAtUtc = e.CreatedAtUtc,
        CorrelationId = e.CorrelationId
    };
}