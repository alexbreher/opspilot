using OpsPilot.Api.Domain.Entities;
using OpsPilot.Api.Models;

namespace OpsPilot.Api.Services;

public class RunbookService
{
    private readonly List<Runbook> _runbooks = new()
    {
        new Runbook
        {
            Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
            ServiceId = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            Title = "Mitigate login failures",
            ContentMarkdown = """
                # Mitigate login failures

                ## Symptoms
                - Users cannot log in
                - Elevated auth latency / timeouts

                ## Steps
                1. Check service health endpoint
                2. Validate dependency latency
                3. Restart auth pods if saturation is confirmed
                4. Roll back latest config if recent change exists

                ## Verification
                - Login succeeds
                - Latency returns to baseline
                """,
            CreatedBy = "system",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
        }
    };

    private readonly ServiceCatalogService _serviceCatalogService;
    private readonly OperationalEventService _operationalEventService;

    public RunbookService(ServiceCatalogService serviceCatalogService, OperationalEventService operationalEventService)
    {
        _serviceCatalogService = serviceCatalogService;
        _operationalEventService = operationalEventService;
    }

    public IEnumerable<RunbookDto> GetByServiceId(Guid serviceId)
    {
        var service = _serviceCatalogService.GetById(serviceId);

        return _runbooks
            .Where(r => r.ServiceId == serviceId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new RunbookDto
            {
                Id = r.Id,
                ServiceId = r.ServiceId,
                ServiceName = service?.Name ?? "Unknown Service",
                Title = r.Title,
                ContentMarkdown = r.ContentMarkdown,
                CreatedBy = r.CreatedBy,
                CreatedAtUtc = r.CreatedAtUtc
            });
    }

    public RunbookDto? GetById(Guid runbookId)
    {
        var r = _runbooks.FirstOrDefault(x => x.Id == runbookId);
        if (r == null) return null;

        var service = _serviceCatalogService.GetById(r.ServiceId);

        return new RunbookDto
        {
            Id = r.Id,
            ServiceId = r.ServiceId,
            ServiceName = service?.Name ?? "Unknown Service",
            Title = r.Title,
            ContentMarkdown = r.ContentMarkdown,
            CreatedBy = r.CreatedBy,
            CreatedAtUtc = r.CreatedAtUtc
        };
    }

    public RunbookDto Create(Guid serviceId, CreateRunbookRequest request)
    {
        var service = _serviceCatalogService.GetById(serviceId);
        if (service == null)
        {
            throw new InvalidOperationException("Service not found.");
        }

        var runbook = new Runbook
        {
            Id = Guid.NewGuid(),
            ServiceId = serviceId,
            Title = request.Title.Trim(),
            ContentMarkdown = request.ContentMarkdown.Trim(),
            CreatedBy = request.CreatedBy.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _runbooks.Add(runbook);

        // Timeline event (service-scoped events will still live on incidents later;
        // for now we write a general operational event against "system").
        _operationalEventService.Add(new Domain.Entities.OperationalEvent
        {
            Id = Guid.NewGuid(),
            ServiceId = service.Id,
            EventType = "RunbookCreated",
            Message = $"Runbook created for service '{service.Name}': {runbook.Title}",
            CreatedBy = runbook.CreatedBy,
            CreatedAtUtc = DateTime.UtcNow,
            CorrelationId = "n/a" // middleware will set correlation for request logs; for now keep simple
        });

        return new RunbookDto
        {
            Id = runbook.Id,
            ServiceId = runbook.ServiceId,
            ServiceName = service.Name,
            Title = runbook.Title,
            ContentMarkdown = runbook.ContentMarkdown,
            CreatedBy = runbook.CreatedBy,
            CreatedAtUtc = runbook.CreatedAtUtc
        };
    }
}