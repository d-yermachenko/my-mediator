using DyoMediator.Notifications;

namespace DyoMediator.Tests.Abstractions.MockImplementation;

internal class SideEffectsHolder()
{
    public List<IDummyDomainEvent> RaisedEvents { get; } = new();

    public void AddEvent(IDummyDomainEvent domainEvent)
    {
        RaisedEvents.Add(domainEvent);
    }

    public void ClearEvents()
    {
        RaisedEvents.Clear();
    }

}

internal interface IDummyDomainEvent
{
    Guid EventId { get; }

    DateTime OccurredOn { get; }

    string Description { get; }
}

public record DummyDomainEvent : IDummyDomainEvent
{
    public Guid EventId { get; private set; } = Guid.CreateVersion7();
    public DateTime OccurredOn { get; private set; } = DateTime.UtcNow;
    public string Description { get; private set; }

    public DummyDomainEvent(Guid eventId, DateTime occurredOn, string description)
    {
        EventId = eventId;
        OccurredOn = occurredOn;
        Description = description;
    }
}

internal class DummyEventHandler : IMyNotificationHandler<DummyDomainEvent>
{
    private readonly SideEffectsHolder _sideEffectsHolder;
    public DummyEventHandler(SideEffectsHolder sideEffectsHolder)
    {
        _sideEffectsHolder = sideEffectsHolder;
    }
    public Task HandleAsync(DummyDomainEvent notification, CancellationToken cancellationToken = default)
    {
        _sideEffectsHolder.AddEvent(notification);
        return Task.CompletedTask;
    }
}

internal class AnotherDummyEventHandler : IMyNotificationHandler<DummyDomainEvent>
{
    private readonly SideEffectsHolder _sideEffectsHolder;
    public AnotherDummyEventHandler(SideEffectsHolder sideEffectsHolder)
    {
        _sideEffectsHolder = sideEffectsHolder;
    }
    public Task HandleAsync(DummyDomainEvent notification, CancellationToken cancellationToken = default)
    {
        _sideEffectsHolder.AddEvent(new DummyDomainEvent(notification.EventId, notification.OccurredOn, notification.Description + " (Handled by AnotherDummyEventHandler)"));
        return Task.CompletedTask;
    }
}
