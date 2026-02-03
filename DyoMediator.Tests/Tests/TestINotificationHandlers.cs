using DyoMediator.Notifications;
using DyoMediator.Tests.Abstractions.MockImplementation;
using Microsoft.Extensions.DependencyInjection;

namespace DyoMediator.Tests;


public class TestINotificationHandlers
{
    [Fact(DisplayName = "Test Dependency Container Has Publisher")]
    public async Task TestDependencyContainerHasPublisher()
    {
        // Arrange
        ServiceCollection services = new();
        services
            .AddMyPublisher<IDummyDomainEvent>(assembly: this.GetType().Assembly);
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        // Act
        IMyPublisher? myPublisher = serviceProvider.GetService<IMyPublisher>();
        // Assert
        Assert.NotNull(myPublisher);
    }


    [Fact(DisplayName = "Test Base Notification Handler")]
    public async Task TestBaseNotificationHandler()
    {
        // Arrange
        ServiceCollection services = new();
        SideEffectsHolder sideEffectsHolder = new();
        services.AddSingleton<SideEffectsHolder>(sideEffectsHolder);
        services
            .AddMyPublisher<IDummyDomainEvent>(types: [typeof(DummyEventHandler), typeof(object), typeof(TestINotificationHandlers)]);

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        DummyDomainEvent dummyDomainEvent = new(Guid.CreateVersion7(), DateTime.UtcNow, "TestEvent");
        IMyPublisher myPublisher = serviceProvider.GetRequiredService<IMyPublisher>();
        // Act
        await myPublisher.PublishAsync<IDummyDomainEvent>(dummyDomainEvent);
        // Assert
        List<IDummyDomainEvent> raisedEvents = sideEffectsHolder.RaisedEvents;
        Assert.Single(raisedEvents);
        Assert.Contains<IDummyDomainEvent>(dummyDomainEvent, raisedEvents);
    }

    [Fact(DisplayName = "Test Two Notification Handlers")]
    public async Task TestTwoNotificationHandlers()
    {
        // Arrange
        ServiceCollection services = new();
        SideEffectsHolder sideEffectsHolder = new();
        services.AddSingleton<SideEffectsHolder>(sideEffectsHolder);
        services
            .AddMyPublisher<IDummyDomainEvent>(assembly: this.GetType().Assembly);

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        DummyDomainEvent dummyDomainEvent = new(Guid.CreateVersion7(), DateTime.UtcNow, "TestEvent");
        IMyPublisher myPublisher = serviceProvider.GetRequiredService<IMyPublisher>();
        // Act
        await myPublisher.PublishAsync<IDummyDomainEvent>(dummyDomainEvent);
        // Assert
        List<IDummyDomainEvent> raisedEvents = sideEffectsHolder.RaisedEvents;
        Assert.True(raisedEvents.Count == 2);
        Assert.Contains(dummyDomainEvent, raisedEvents);
        Assert.True(raisedEvents.FindAll(e => e.Description.Contains(nameof(AnotherDummyEventHandler))).Count == 1);
    }

    [Fact(DisplayName = "Test Two Notification Handlers")]
    public async Task IntegrationAndDomainEvents()
    {
        // Arrange
        ServiceCollection services = new();
        IntegrationSideEffectsHolder intgrationEventsHolder = new();
        SideEffectsHolder sideEffectsHolder = new();
        services.AddSingleton<SideEffectsHolder>(sideEffectsHolder);
        services.AddSingleton<IntegrationSideEffectsHolder>(intgrationEventsHolder);

        services.AddMyPublisher<IDummyDomainEvent>(assembly: this.GetType().Assembly);
        services.AddMyPublisher<IDummyIntegrationEvent>(assembly: this.GetType().Assembly);

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        DummyDomainEvent dummyDomainEvent = new(Guid.CreateVersion7(), DateTime.UtcNow, "TestEvent");
        DummyIntegrationEvent dummyIntegrationEvent = new(Guid.CreateVersion7(), DateTime.UtcNow, "TestIntegrationEvent");

        IMyPublisher myPublisher = serviceProvider.GetRequiredService<IMyPublisher>();
        // Act
        await myPublisher.PublishAsync(dummyDomainEvent);
        await myPublisher.PublishAsync(dummyIntegrationEvent);

        // Assert
        List<IDummyDomainEvent> raisedEvents = sideEffectsHolder.RaisedEvents;
        Assert.True(raisedEvents.Count == 2);
        Assert.Contains(dummyDomainEvent, raisedEvents);
        Assert.True(raisedEvents.FindAll(e => e.Description.Contains(nameof(AnotherDummyEventHandler))).Count == 1);

        List<IDummyIntegrationEvent> processedEvents = intgrationEventsHolder.ProcessedEvents;
        Assert.Single(processedEvents);
        Assert.Contains(dummyIntegrationEvent, processedEvents);
        Assert.True(processedEvents[0].ProcessedAt.HasValue);
        Assert.Equal("DummyIntegrationEventHandler", processedEvents[0].ProcessorName);
    }
}
