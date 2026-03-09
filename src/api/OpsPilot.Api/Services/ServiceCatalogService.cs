using OpsPilot.Api.Domain.Entities;
using OpsPilot.Api.Models;

namespace OpsPilot.Api.Services;

public class ServiceCatalogService
{
    private readonly List<Service> _services = new()
    {
        new Service
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Member Portal",
            Description = "Customer-facing web portal for members",
            Owner = "Digital Experience Team",
            IsActive = true
        },
        new Service
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Notification Engine",
            Description = "Handles outbound operational notifications",
            Owner = "Platform Operations",
            IsActive = true
        }
    };

    public IEnumerable<ServiceDto> GetAll()
    {
        return _services.Select(s => new ServiceDto
        {
            Id = s.Id,
            TenantId = s.TenantId,
            Name = s.Name,
            Description = s.Description,
            Owner = s.Owner,
            IsActive = s.IsActive
        });
    }

    public ServiceDto Create(CreateServiceRequest request)
    {
        var newService = new Service
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Owner = request.Owner.Trim(),
            IsActive = true
        };

        _services.Add(newService);

        return new ServiceDto
        {
            Id = newService.Id,
            TenantId = newService.TenantId,
            Name = newService.Name,
            Description = newService.Description,
            Owner = newService.Owner,
            IsActive = newService.IsActive
        };
    }
}