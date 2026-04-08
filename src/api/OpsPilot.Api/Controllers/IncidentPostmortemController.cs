using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/incidents/{incidentId:guid}/postmortem")]
public class IncidentPostmortemController : ControllerBase
{
    private readonly PostmortemService _postmortemService;

    public IncidentPostmortemController(PostmortemService postmortemService)
    {
        _postmortemService = postmortemService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PostmortemDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get(Guid incidentId)
    {
        var pm = _postmortemService.GetByIncidentId(incidentId);
        if (pm == null) return NotFound("Postmortem not found for this incident.");

        return Ok(pm);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PostmortemDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create(Guid incidentId, [FromBody] CreatePostmortemRequest request)
    {
        if (incidentId == Guid.Empty) return BadRequest("IncidentId is required.");
        if (request == null) return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Summary)) return BadRequest("Summary is required.");
        if (string.IsNullOrWhiteSpace(request.RootCause)) return BadRequest("RootCause is required.");
        if (string.IsNullOrWhiteSpace(request.Impact)) return BadRequest("Impact is required.");
        if (string.IsNullOrWhiteSpace(request.Resolution)) return BadRequest("Resolution is required.");
        if (string.IsNullOrWhiteSpace(request.LessonsLearned)) return BadRequest("LessonsLearned is required.");
        if (string.IsNullOrWhiteSpace(request.CreatedBy)) return BadRequest("CreatedBy is required.");

        try
        {
            var created = _postmortemService.CreateForIncident(incidentId, request);
            return CreatedAtAction(nameof(Get), new { incidentId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}