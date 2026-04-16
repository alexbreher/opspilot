namespace OpsPilot.Api.Models;

public class ProcessedEventCheckRequest
{
    public string EventId { get; set; } = string.Empty;
}

public class ProcessedEventCheckResponse
{
    public string EventId { get; set; } = string.Empty;
    public bool AlreadyProcessed { get; set; }
}

public class ProcessedEventMarkRequest
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}