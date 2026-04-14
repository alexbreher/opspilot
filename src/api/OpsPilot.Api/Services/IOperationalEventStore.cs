using OpsPilot.Api.Domain.Entities;

namespace OpsPilot.Api.Services;

public interface IOperationalEventStore
{
    IReadOnlyList<OperationalEvent> GetAll();
    void Add(OperationalEvent operationalEvent);
}