namespace OpsPilot.Api.Messaging;

public interface IEventQueue
{
    void Enqueue(object message);
    bool TryDequeue(out object? message);
}