using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Messaging;
using OpsPilot.Api.Models;
using OpsPilot.Api.Security;

namespace OpsPilot.Api.Controllers;

[DevelopmentOnly]
[ApiController]
[Route("api/internal/inproc-queue")]
public class InternalInProcessQueueController : ControllerBase
{
    private readonly IBackgroundEventQueue _queue;

    public InternalInProcessQueueController(IBackgroundEventQueue queue)
    {
        _queue = queue;
    }

    [HttpPost("enqueue")]
    public async Task<IActionResult> Enqueue([FromBody] EventEnvelopeV2 env, CancellationToken ct)
    {
        if(env == null) return BadRequest("Envelope is required.");
        if (string.IsNullOrWhiteSpace(env.Type)) return BadRequest("Type is required.");
        if (string.IsNullOrWhiteSpace(env.EventId)) return BadRequest("EventId is required.");

        await _queue.EnqueueAsync(env, ct);
        return Accepted();
    }
}