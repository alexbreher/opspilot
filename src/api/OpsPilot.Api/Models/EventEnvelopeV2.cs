using System.Text.Json;

namespace OpsPilot.Api.Models;

public class EventEnvelopeV2
{
    public string Type { get; set; } = string.Empty;       // e.g. OpsPilot.Api.Contracts.IncidentCreatedEvent
    public string EventId { get; set; } = string.Empty;    // extracted for idempotency
    public JsonElement Payload { get; set; }               // full payload
}