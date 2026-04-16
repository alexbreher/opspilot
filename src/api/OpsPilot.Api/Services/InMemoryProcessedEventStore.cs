using System.Collections.Concurrent;

namespace OpsPilot.Api.Services;

public class InMemoryProcessedEventStore : IProcessedEventStore
{
    private readonly ConcurrentDictionary<string, ProcessedEventRecord> _processed = new(StringComparer.OrdinalIgnoreCase);

    public bool Exists(string eventId)  => !string.IsNullOrWhiteSpace(eventId) && _processed.ContainsKey(eventId);

    public void Mark(string eventId, string eventType, DateTime processedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(eventId)) return;

        var record = new ProcessedEventRecord(
            EventId: eventId.Trim(),
            EventType: string.IsNullOrWhiteSpace(eventType) ? "Unknown" : eventType.Trim(),
            ProcessedAtUtc: processedAtUtc == default ? DateTime.UtcNow : processedAtUtc
            );
        _processed.TryAdd(record.EventId, record);
    }
    public IReadOnlyList<ProcessedEventRecord> GetAll()  => _processed.Values.OrderByDescending(x => x.ProcessedAtUtc).ToList();
}