using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly IncidentService _incidentService;
    private readonly IIncidentTimeLineStore _timelineStore;

    public IncidentsController(IncidentService incidentService, IIncidentTimeLineStore timelineStore)
    {
        _incidentService = incidentService;
        _timelineStore = timelineStore;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<IncidentDto>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var result = _incidentService.GetAll();
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(IncidentDto),StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateIncidentRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required");
        }
        if (request.ServiceId == Guid.Empty)
        {
            return BadRequest("Service ID is required");
        }
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required");
        }
        if (string.IsNullOrWhiteSpace(request.Severity))
        {
            return BadRequest("Severity is required");
        }

        var allowedSeverities = new[] { "Low", "Medium", "High", "Critical" };

        if (!allowedSeverities.Contains(request.Severity.Trim(),StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest("Severity must be one of: Low, Medium, High, Critical.");
        }
        try
        {
            var created = _incidentService.Create(request);
            return CreatedAtAction(nameof(GetAll), new {id = created.Id},created);
        }
        catch(InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{incidentId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdateStatus(Guid incidentId, [FromBody] UpdateIncidentStatusRequest request)
    {
        if (incidentId == Guid.Empty) 
        {
            return BadRequest("IncidentId is required");
        }
        if (request == null)
        {
            return BadRequest("Request body is required");
        }
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest("Status is required");
        }
        if (string.IsNullOrWhiteSpace(request.UpdatedBy))
        {
            return BadRequest("Updatedby is required");
        }
        try
        {
            var updated = _incidentService.UpdateStatus(incidentId, request.Status, request.UpdatedBy, _timelineStore);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}