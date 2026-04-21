using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class WorkerStatusController : ControllerBase
{
    private readonly OperationalEventService _ops;

    public WorkerStatusController(OperationalEventService ops)
    {
        _ops = ops;
    }
    [HttpGet("heartbeat")]
    public IActionResult GetLastHeartbeat()
    {
        var last = _ops.GetLatest(200).FirstOrDefault(e => e.EventType == "WorkerHeartbeat");

        if (last == null)
            return NotFound("No heartbeat receiced yet");

        return Ok(new
        {
            last.CreatedAtUtc,
            last.Message,
            last.CreatedBy,
            last.CorrelationId
        });
    }
}