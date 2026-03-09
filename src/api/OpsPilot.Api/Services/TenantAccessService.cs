using OpsPilot.Api.Domain.Entities;
using OpsPilot.Api.Models;

namespace OpsPilot.Api.Services;

public class TenantAccessService : ITenantAccessService
{
    public IEnumerable<TenantMembershipDto> GetAllMemberships()
    {
        var tenants = new List<Tenant>
        {
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Acme Health", Slug = "acme-health", IsActive = true },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Northwind Ops", Slug = "northwind-ops", IsActive = true }
        };

        var users = new List<AppUser>
        {
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Email = "alex@acme.com", DisplayName = "Alex Bravo", IsActive = true },
            new() { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Email = "sre@northwind.com", DisplayName = "Northwind SRE", IsActive = true }
        };

        var roles = new List<Role>
        {
            new() { Id = Guid.Parse("99999999-9999-9999-9999-999999999991"), Name = "TenantAdmin" },
            new() { Id = Guid.Parse("99999999-9999-9999-9999-999999999992"), Name = "Engineer" }
        };

        var memberships = new List<Membership>
        {
            new()
            {
                TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                RoleId = Guid.Parse("99999999-9999-9999-9999-999999999991"),
                IsActive = true
            },
            new()
            {
                TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                RoleId = Guid.Parse("99999999-9999-9999-9999-999999999992"),
                IsActive = true
            }
        };

        var result =
            from membership in memberships
            join tenant in tenants on membership.TenantId equals tenant.Id
            join user in users on membership.UserId equals user.Id
            join role in roles on membership.RoleId equals role.Id
            select new TenantMembershipDto
            {
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                TenantSlug = tenant.Slug,
                UserId = user.Id,
                UserEmail = user.Email,
                UserDisplayName = user.DisplayName,
                RoleId = role.Id,
                RoleName = role.Name,
                IsActive = membership.IsActive
            };

        return result;
    }
}