namespace OpsPilot.Api.Models;

public class CreateIncidentTimelineEventRequest
{
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}