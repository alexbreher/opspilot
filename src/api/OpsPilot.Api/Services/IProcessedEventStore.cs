namespace OpsPilot.Api.Services;

public interface IProcessedEventStore
{
    bool Exists(string eventId);
    void Mark(string eventId, string eventType, DateTime processedAtUtc);
    IReadOnlyList<ProcessedEventRecord> GetAll();
}

public record ProcessedEventRecord(string EventId, string EventType, DateTime ProcessedAtUtc);