using OpsPilot.Api.Domain.Entities;
using OpsPilot.Api.Models;

namespace OpsPilot.Api.Services;

public class IncidentService
{
    private readonly List<Incident> _incidents = new()
    {
        new Incident
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
            ServiceId = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            Title = "Member login failures",
            Description = "Users are unable to authenticate in the member portal.",
            Severity = "High",
            Status = "Open",
            CreatedAtUtc = DateTime.UtcNow.AddHours(-4)
        },
        new Incident
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
            ServiceId = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            Title = "Notification delays",
            Description = "Outbound notification processing is delayed by more than 10 minutes.",
            Severity = "Medium",
            Status = "Open",
            CreatedAtUtc = DateTime.UtcNow.AddHours(-2)
        }
    };

    private readonly ServiceCatalogService _serviceCatalogService;

    public IncidentService(ServiceCatalogService serviceCatalogService)
    {
        _serviceCatalogService = serviceCatalogService;
    }

    public IEnumerable<IncidentDto> GetAll()
    {
        return _incidents.Select(i =>
        {
            var service = _serviceCatalogService.GetById(i.ServiceId);

            return new IncidentDto
            {
                Id = i.Id,
                ServiceId = i.ServiceId,
                ServiceName = service?.Name ?? "Unknown Service",
                Title = i.Title,
                Description = i.Description,
                Severity = i.Severity,
                Status = i.Status,
                CreatedAtUtc = i.CreatedAtUtc
            };
        });
    }

    public IncidentDto Create(CreateIncidentRequest request)
    {
        var service = _serviceCatalogService.GetById(request.ServiceId);

        if (service == null)
        {
            throw new InvalidOperationException("Service not found.");
        }

        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            ServiceId = request.ServiceId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = request.Severity.Trim(),
            Status = "Open",
            CreatedAtUtc = DateTime.UtcNow
        };

        _incidents.Add(incident);

        return new IncidentDto
        {
            Id = incident.Id,
            ServiceId = incident.ServiceId,
            ServiceName = service.Name,
            Title = incident.Title,
            Description = incident.Description,
            Severity = incident.Severity,
            Status = incident.Status,
            CreatedAtUtc = incident.CreatedAtUtc
        };
    }

    public bool Exists(Guid incidentId)
    {
        return _incidents.Any(i => i.Id == incidentId);
    }


}