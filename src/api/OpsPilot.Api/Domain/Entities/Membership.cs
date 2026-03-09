namespace OpsPilot.Api.Domain.Entities;

public class Membership
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; } = true;
}