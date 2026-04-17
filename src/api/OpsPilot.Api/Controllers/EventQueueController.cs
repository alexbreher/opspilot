using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Messaging;
using System.Text.Json;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/internal/event-queue")]
public class EventQueueController : ControllerBase
{
    private readonly IEventQueue _queue;

    public EventQueueController(IEventQueue queue)
    {
        _queue = queue;
    }

    // Existing: returns an envelope { type, payload }
    [HttpPost("dequeue")]
    public IActionResult DequeueOne()
    {
        if (_queue.TryDequeue(out var msg) && msg != null)
        {
            return Ok(new
            {
                type = msg.GetType().FullName,
                payload = msg
            });
        }

        return NoContent();
    }

    // Existing: enqueue raw payload (keep for flexibility)
    // Recommended: use JsonElement so you preserve JSON structure reliably.
    [HttpPost("enqueue")]
    public IActionResult Enqueue([FromBody] JsonElement payload)
    {
        if (payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return BadRequest("Payload is required.");

        _queue.Enqueue(payload);
        return Accepted();
    }

    // NEW: enqueue using the exact envelope returned by dequeue.
    // This makes replay/idempotency testing deterministic.
    [HttpPost("enqueue-envelope")]
    public IActionResult EnqueueEnvelope([FromBody] EventEnvelope envelope)
    {
        if (envelope == null)
            return BadRequest("Envelope is required.");

        if (envelope.Payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return BadRequest("Envelope.payload is required.");

        // We intentionally ignore envelope.type and enqueue only the payload,
        // because the worker expects eventId to live at payload root.
        _queue.Enqueue(envelope.Payload);
        return Accepted();
    }

    public sealed class EventEnvelope
    {
        public string? Type { get; set; }
        public JsonElement Payload { get; set; }
    }
}