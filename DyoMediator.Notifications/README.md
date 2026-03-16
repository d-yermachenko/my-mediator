# DyoMediator.Notifications

Notification (publish/subscribe) infrastructure for the DyoMediator ecosystem.  
Allows you to publish events to multiple handlers using dependency injection, similar to MediatR's notification pattern.

## Responsibilities

- Defines `IMyPublisher<TNotificationBaseType>` for publishing notifications/events.
- Defines `IMyNotificationHandler<TNotificationType>` for handling notifications.
- Provides DI extensions to register handlers and the publisher.
- Supports handler resolution for both base and derived notification types.

## Installation

Add a project reference to `DyoMediator.Notifications` and register with DI:
```csharp
using DyoMediator.Notifications; 
using Microsoft.Extensions.DependencyInjection;

services.AddMyPublisher<IMyDomainEvent>(assembly: typeof(MyHandler).Assembly);
```


## Usage Example

### 1. Define a Notification/Event
```csharp
public interface IDummyDomainEvent { 
	Guid EventId { get; }
	DateTime OccurredOn { get; }
	string Description { get; } 
}
```

### 2. Implement a Notification Handler
```csharp
public class DummyEventHandler : IMyNotificationHandler<IDummyDomainEvent> 
{ 
	private readonly SideEffectsHolder _sideEffectsHolder;

	public DummyEventHandler(SideEffectsHolder sideEffectsHolder) => _sideEffectsHolder = sideEffectsHolder;

	public Task HandleAsync(IDummyDomainEvent notification, CancellationToken cancellationToken = default)
	{
		_sideEffectsHolder.AddEvent(notification);
		return Task.CompletedTask;
	}
}
```
### 3. Register Handlers and Publisher
```csharp
IServiceCollection services = new ServiceCollection();
services.AddMyPublisher<IDummyDomainEvent>(assembly: typeof(DummyEventHandler).Assembly);
var provider = services.BuildServiceProvider();

```
### 4. Publish a Notification
```csharp
var publisher = provider.GetRequiredService<IMyPublisher<IDummyDomainEvent>>();
var domainEvent = new DummyDomainEvent(Guid.NewGuid(), DateTime.UtcNow, "TestEvent");
await publisher.PublishAsync(domainEvent);
```
### 5. Test Example
```csharp
// Arrange 
var sideEffects = provider.GetRequiredService<SideEffectsHolder>();
// Act
await publisher.PublishAsync(domainEvent);
// Assert
Assert.Single(sideEffects.RaisedEvents);
Assert.Contains(domainEvent, sideEffects.RaisedEvents);
```
## Features

- Handlers can be registered for base interfaces; all matching handlers are invoked for derived notifications.
- Handlers are resolved and invoked in parallel.
- Supports both single and multiple notification publishing.

## Notes

- If you register a handler for a base type (e.g., `IMyNotificationHandler<IDummyDomainEvent>`) and publish a derived type (e.g., `DummyDomainEvent`), the handler will be invoked.
- If no handlers are registered, publishing is a no-op.
- Exceptions in handlers are aggregated by `Task.WhenAll`.

## See Also

- [DyoMediator.Abstraction](../DyoMediator.Abstraction/README.md)
- [DyoMediator.Mediator](../DyoMediator.Mediator/README.md)


