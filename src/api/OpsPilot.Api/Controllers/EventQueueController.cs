using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Messaging;

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
    [HttpPost("dequeue")]
    public IActionResult DequeueOne()
    {
        if (_queue.TryDequeue(out var msg) && msg != null)
        {
            // We return the CLR type so the worker can route it
            return Ok(new
            {
                type = msg.GetType().FullName,
                payload = msg
            });
        }
        return NoContent();
    }
    [HttpPost("enqueue")]
    public IActionResult Enqueue([FromBody] object payload)
    {
        if (payload == null) return BadRequest("Payload is required.");
        _queue.Enqueue(payload);
        return Accepted();
    }
}