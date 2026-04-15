using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using OpsPilot.Api.Domain.Entities;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/internal/operational-events")]
public class InternalOperationalEventsController : ControllerBase
{
    private readonly OperationalEventService _operationalEventService;

    public InternalOperationalEventsController(OperationalEventService operationalEventService)
    {
        _operationalEventService = operationalEventService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Ingest([FromBody] IngestOperationalEventRequest request)
    {
        if (request == null) return BadRequest("Request body is required.");
        if (string.IsNullOrWhiteSpace(request.EventType)) return BadRequest("EventType is required.");
        if (string.IsNullOrWhiteSpace(request.Message)) return BadRequest("Message is required.");
        if (string.IsNullOrWhiteSpace(request.CreatedBy)) return BadRequest("CreatedBy is required.");

        var e = new OperationalEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ServiceId = request.ServiceId,
            IncidentId = request.IncidentId,
            EventType = request.EventType.Trim(),
            Message = request.SourceEventId == null
                ? request.Message.Trim()
                : $"{request.Message.Trim()} (SourceEventId={request.SourceEventId})",
            CreatedBy = request.CreatedBy.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            CorrelationId = string.IsNullOrWhiteSpace(request.CorrelationId) ? "n/a" : request.CorrelationId.Trim()
        };

        _operationalEventService.Add(e);

        // 202 = accepted for async-style ingestion
        return Accepted();
    }
}