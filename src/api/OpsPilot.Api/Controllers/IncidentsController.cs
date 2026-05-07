using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;
using OpsPilot.Api.Contracts;
using OpsPilot.Api.Messaging;
using OpsPilot.Api.Middleware;
using System.Text.Json;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly IncidentService _incidentService;
    private readonly IIncidentTimeLineStore _timelineStore;
    private readonly IEventBus _eventBus;
    private readonly IEventQueueV2 _queueV2;
    private readonly IBackgroundEventQueue _bgQueue;
    private readonly IMessageBusPublisher _busPublisher;

    public IncidentsController(
        IncidentService incidentService,
        IIncidentTimeLineStore timelineStore,
        IEventBus eventBus,
        IEventQueueV2 queueV2,
        IBackgroundEventQueue bgQueue,
        IMessageBusPublisher busPublisher)
    {
        _incidentService = incidentService;
        _timelineStore = timelineStore;
        _eventBus = eventBus;
        _queueV2 = queueV2;
        _bgQueue = bgQueue;
        _busPublisher = busPublisher;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<IncidentDto>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var result = _incidentService.GetAll();
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateIncidentRequest request)
    {
        if (request == null) return BadRequest("Request body is required");
        if (request.ServiceId == Guid.Empty) return BadRequest("Service ID is required");
        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Title is required");
        if (string.IsNullOrWhiteSpace(request.Severity)) return BadRequest("Severity is required");

        var allowedSeverities = new[] { "Low", "Medium", "High", "Critical" };
        if (!allowedSeverities.Contains(request.Severity.Trim(), StringComparer.OrdinalIgnoreCase))
            return BadRequest("Severity must be one of: Low, Medium, High, Critical.");

        try
        {
            var created = _incidentService.Create(request);

            var correlationId =
                HttpContext.Response.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName, out var corr)
                    ? corr.ToString()
                    : Guid.NewGuid().ToString("N");

            var payload = JsonSerializer.SerializeToElement(new
            {
                eventId = Guid.NewGuid().ToString(),
                createdAtUtc = DateTime.UtcNow,
                correlationId,
                incidentId = created.Id,
                serviceId = created.ServiceId,
                title = created.Title,
                severity = created.Severity
            });

            // Day 18: in-proc processor
            await _bgQueue.EnqueueAsync(new EventEnvelopeV2
            {
                Type = "OpsPilot.Api.Contracts.IncidentCreatedEvent",
                EventId = payload.GetProperty("eventId").GetString()!,
                Payload = payload
            });

            // ✅ Day 19: publish to RabbitMQ (no-op unless Transport=RabbitMq)
            await _busPublisher.PublishIncidentCreatedAsync(
                new IncidentCreatedMessage(
                    EventId: payload.GetProperty("eventId").GetString()!,
                    CreatedAtUtc: DateTime.UtcNow,
                    CorrelationId: correlationId,
                    IncidentId: created.Id,
                    ServiceId: created.ServiceId,
                    Title: created.Title,
                    Severity: created.Severity),
                HttpContext.RequestAborted);

            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{incidentId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid incidentId, [FromBody] UpdateIncidentStatusRequest request)
    {
        if (incidentId == Guid.Empty) return BadRequest("IncidentId is required");
        if (request == null) return BadRequest("Request body is required");
        if (string.IsNullOrWhiteSpace(request.Status)) return BadRequest("Status is required");
        if (string.IsNullOrWhiteSpace(request.UpdatedBy)) return BadRequest("UpdatedBy is required");

        try
        {
            var incidentEntity = _incidentService.GetEntityById(incidentId);
            var oldStatus = incidentEntity?.Status ?? "Unknown";

            var updated = _incidentService.UpdateStatus(incidentId, request.Status, request.UpdatedBy, _timelineStore);

            var correlationId =
                HttpContext.Response.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName, out var corr)
                    ? corr.ToString()
                    : Guid.NewGuid().ToString("N");

            var payload = JsonSerializer.SerializeToElement(new
            {
                eventId = Guid.NewGuid().ToString(),
                createdAtUtc = DateTime.UtcNow,
                correlationId,
                incidentId,
                oldStatus,
                newStatus = updated.Status,
                updatedBy = request.UpdatedBy
            });

            // Day 18: in-proc processor
            await _bgQueue.EnqueueAsync(new EventEnvelopeV2
            {
                Type = "OpsPilot.Api.Contracts.IncidentStatusChangedEvent",
                EventId = payload.GetProperty("eventId").GetString()!,
                Payload = payload
            });

            // ✅ Day 19: publish to RabbitMQ (no-op unless Transport=RabbitMq)
            await _busPublisher.PublishIncidentStatusChangedAsync(
                new IncidentStatusChangedMessage(
                    EventId: payload.GetProperty("eventId").GetString()!,
                    CreatedAtUtc: DateTime.UtcNow,
                    CorrelationId: correlationId,
                    IncidentId: incidentId,
                    OldStatus: oldStatus,
                    NewStatus: updated.Status,
                    UpdatedBy: request.UpdatedBy),
                HttpContext.RequestAborted);

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}