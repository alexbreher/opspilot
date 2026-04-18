using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Messaging;
using OpsPilot.Api.Models;
using System.Text.Json;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/internal/event-queue/v2")]
public class EventQueueV2Controller : ControllerBase
{
    private readonly IEventQueueV2 _queue;

    public EventQueueV2Controller(IEventQueueV2 queue)
    {
        _queue = queue;
    }

    [HttpPost("dequeue")]
    public IActionResult DequeueOne()
    {
        if (_queue.TryDequeue(out var env) && env != null)
        {
            return Ok(env);
        }
        return NoContent();
    }

    [HttpPost("enqueue")]
    public IActionResult Enqueue([FromBody] EventEnvelopeV2 env)
    {
        if (env == null) return BadRequest("Envelope is required.");
        if (string.IsNullOrWhiteSpace(env.Type)) return BadRequest("Type is required.");
        if (string.IsNullOrWhiteSpace(env.EventId)) return BadRequest("EventId is required.");
        if (env.Payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null) return BadRequest("Payload is required.");

        _queue.Enqueue(env);
        return Accepted();
    }
}