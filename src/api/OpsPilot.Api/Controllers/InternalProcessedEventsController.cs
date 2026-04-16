using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/internal/processed-events")]
public class InternalProcessedEventsController : ControllerBase
{
    private readonly IProcessedEventStore _store;

    public InternalProcessedEventsController(IProcessedEventStore store)
    {
        _store = store;
    }
    [HttpPost("check")]
    [ProducesResponseType(typeof(ProcessedEventCheckResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Check([FromBody] ProcessedEventCheckRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.EventId))
            return BadRequest("EventId is required");

        var exists = _store.Exists(request.EventId);
        return Ok(new ProcessedEventCheckResponse 
        { 
            EventId = request.EventId.Trim(),
            AlreadyProcessed = exists
        });
    }
    [HttpPost("mark")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Mark([FromBody] ProcessedEventMarkRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.EventId))
            return BadRequest("EventId is required.");

        _store.Mark(request.EventId, request.EventType, request.ProcessedAtUtc);

        return Accepted();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var records = _store.GetAll().Select(r => new { r.EventId, r.EventType, r.ProcessedAtUtc });

        return Ok(records);
    }
}