namespace OpsPilot.Api.Models;

public class TenantMembershipDto
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;

    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}