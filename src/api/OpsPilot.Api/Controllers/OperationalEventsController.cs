using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OperationalEventsController : ControllerBase
{
    private OperationalEventService _service;

    public OperationalEventsController(OperationalEventService service)
    {
        _service = service;
    }

    // GET /api/OperationalEvents?limit=50
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OperationalEventDto>), StatusCodes.Status200OK)]
    public IActionResult GetLatest([FromQuery] int limit = 50)
    {
        var result = _service.GetLatest(limit);
        return Ok(result);
    }

    // GET /api/OperationalEvents/service/{serviceId}?limit=50
    [HttpGet("service/{serviceId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<OperationalEventDto>),StatusCodes.Status200OK)]
    public IActionResult GetByService(Guid serviceId, [FromQuery] int limit = 50)
    {
        var result = _service.GetByService(serviceId, limit);
        return Ok(result);
    }

}