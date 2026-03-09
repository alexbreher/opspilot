using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceCatalogController: ControllerBase
{
    private readonly ServiceCatalogService _serviceCatalogService;

    public ServiceCatalogController(ServiceCatalogService serviceCatalogService)
    {
        _serviceCatalogService = serviceCatalogService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServiceDto>),StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var result = _serviceCatalogService.GetAll();
        return Ok(result);
    }
    [HttpPost]
    [ProducesResponseType(typeof(ServiceDto),StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateServiceRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required");
        }
        if (request.TenantId == Guid.Empty)
        {
            return BadRequest("Tenant Id is required");
        }
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required");
        }
        if (string.IsNullOrWhiteSpace(request.Owner))
        {
            return BadRequest("Owner is required");
        }

        var created = _serviceCatalogService.Create(request);

        return CreatedAtAction(nameof(GetAll), new {id = created.Id}, created);
    }
}