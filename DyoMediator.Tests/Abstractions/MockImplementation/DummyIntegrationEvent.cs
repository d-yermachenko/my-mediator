using DyoMediator.Notifications;

namespace DyoMediator.Tests.Abstractions.MockImplementation;

internal class IntegrationSideEffectsHolder()
{
    public List<IDummyIntegrationEvent> ProcessedEvents = new();
    public void AddEvent(IDummyIntegrationEvent integrationEvent)
    {
        ProcessedEvents.Add(integrationEvent);
    }
}

internal interface IDummyIntegrationEvent
{
    Guid EventId { get; }

    DateTime OccurredOn { get; }

    DateTime? ProcessedAt { get; }

    string Description { get; }

    string? ProcessorName { get;  }

    void MarkAsProcessed(string processorName, DateTime processedAt);

}

internal class DummyIntegrationEvent : IDummyIntegrationEvent
{
    public Guid EventId { get; private set; } = Guid.NewGuid();
    public DateTime OccurredOn { get; private set; } = DateTime.UtcNow;
    public string Description { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ProcessorName { get; private set; }

    public DummyIntegrationEvent(Guid eventId, DateTime occurredOn, string description)
    {
        EventId = eventId;
        OccurredOn = occurredOn;
        Description = description;
    }

    public void MarkAsProcessed(string processorName, DateTime processedAt)
    {
        ProcessedAt = processedAt;
        ProcessorName = processorName;
    }
}

internal class DummyIntegrationEventHandler : IMyNotificationHandler<DummyIntegrationEvent>
{
    private readonly IntegrationSideEffectsHolder _sideEffectsHolder;
    public DummyIntegrationEventHandler(IntegrationSideEffectsHolder sideEffectsHolder)
    {
        _sideEffectsHolder = sideEffectsHolder;
    }
    public Task HandleAsync(DummyIntegrationEvent notification, CancellationToken cancellationToken = default)
    {
        notification.MarkAsProcessed(nameof(DummyIntegrationEventHandler), DateTime.UtcNow);
        _sideEffectsHolder.AddEvent(notification);
        return Task.CompletedTask;
    }
}
