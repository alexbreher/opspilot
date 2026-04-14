namespace OpsPilot.Api.Messaging;

public interface IEventBus
{
    void Publish<T>(T @event) where T : class;
    void Subscribe<T>(Action<T> handler) where T : class;
}