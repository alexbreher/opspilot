using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/incidents/{incidentId:guid}/timeline")]
public class IncidentTimelineController : ControllerBase
{
    private readonly IncidentTimelineService _incidentTimelineService;

    public IncidentTimelineController(IncidentTimelineService incidentTimelineService)
    {
        _incidentTimelineService = incidentTimelineService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<IncidentTimelineEventDto>), StatusCodes.Status200OK)]
    public IActionResult GetByIncidentId(Guid incidentId)
    {
        var result = _incidentTimelineService.GetByIncidentId(incidentId);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(IncidentTimelineEventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create(Guid incidentId, [FromBody] CreateIncidentTimelineEventRequest request)
    {
        if (incidentId == Guid.Empty)
        {
            return BadRequest("IncidentId is required.");
        }

        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.EventType))
        {
            return BadRequest("EventType is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message is required.");
        }

        if (string.IsNullOrWhiteSpace(request.CreatedBy))
        {
            return BadRequest("CreatedBy is required.");
        }

        try
        {
            var created = _incidentTimelineService.Create(incidentId, request);
            return CreatedAtAction(nameof(GetByIncidentId), new { incidentId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}