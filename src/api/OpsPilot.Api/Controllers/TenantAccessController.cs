using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantAccessController : ControllerBase
{
    private readonly TenantAccessService _tenantAccessService;

    public TenantAccessController(TenantAccessService tenantAccessService)
    {
        _tenantAccessService = tenantAccessService;
    }

    [HttpGet("memberships")]
    [ProducesResponseType(typeof(IEnumerable<TenantMembershipDto>), StatusCodes.Status200OK)]
    public IActionResult GetMemberships()
    {
        var result = _tenantAccessService.GetAllMemberships();
        return Ok(result);
    }
}