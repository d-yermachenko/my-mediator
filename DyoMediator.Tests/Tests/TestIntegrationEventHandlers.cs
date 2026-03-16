using DyoMediator.Notifications;
using DyoMediator.Tests.Abstractions.MockImplementation;
using Microsoft.Extensions.DependencyInjection;

namespace DyoMediator.Tests;

public class TestIntegrationEventHandlers
{
    [Fact]
    public async Task Handlers_Registered_For_Base_Interface_Are_Invoked_For_Derived_Event()
    {
        // Arrange
        var services = new ServiceCollection();
        var sideEffects = new IntegrationSideEffectsHolder();
        services.AddSingleton(sideEffects);

        // Register handlers using AddMyPublisher for the base interface
        services.AddMyPublisher<IDummyIntegrationEvent>(assembly: this.GetType().Assembly);

        var sp = services.BuildServiceProvider();
        var publisher = sp.GetRequiredService<IMyPublisher<IDummyIntegrationEvent>>();

        var evt = new DummyIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, "desc");

        // Act
        await publisher.PublishAsync(evt, CancellationToken.None);

        // Assert
        Assert.Single(sideEffects.ProcessedEvents);
        Assert.Contains(evt, sideEffects.ProcessedEvents);
    }
}
