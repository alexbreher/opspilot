using OpsPilot.Api.Models;

namespace OpsPilot.Api.Services;

public interface ITenantAccessService
{
    IEnumerable<TenantMembershipDto> GetAllMemberships();
}