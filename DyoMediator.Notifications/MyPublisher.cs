using Microsoft.Extensions.DependencyInjection;

namespace DyoMediator.Notifications;

public class MyPublisher<TNotificationBaseType> : IMyPublisher<TNotificationBaseType>
    where TNotificationBaseType : notnull
{
    private readonly IServiceProvider serviceProvider;

    public MyPublisher(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        this.serviceProvider = serviceProvider;
    }

    public Task PublishAsync<TMessageType>(TMessageType notification, CancellationToken cancellationToken)
        where TMessageType :  notnull, TNotificationBaseType
    {
        ArgumentNullException.ThrowIfNull(notification);

        // Resolve handlers registered specifically for the message type
        var exactHandlers = serviceProvider.GetServices<IMyNotificationHandler<TMessageType>>();

        // Resolve handlers registered for the base notification type
        var baseHandlers = serviceProvider.GetServices<IMyNotificationHandler<TNotificationBaseType>>();

        // If there are any handlers registered for the base type, invoke them (they accept TNotificationBaseType)
        var tasks = new List<Task>();

        tasks.AddRange(exactHandlers.Select(h => h.HandleAsync(notification, cancellationToken)));

        // For base handlers, we need to pass notification as TNotificationBaseType
        tasks.AddRange(baseHandlers.Select(h => h.HandleAsync((TNotificationBaseType)notification, cancellationToken)));

        // Fallback: if there are no handlers found via generic resolution, try reflection-based resolution
        if (!tasks.Any())
        {
            Type serviceType = typeof(IMyNotificationHandler<>).MakeGenericType(notification.GetType());
            var handlers = serviceProvider.GetServices(serviceType);
            tasks.AddRange(handlers.Select(handler => CallService(handler, notification, cancellationToken)));
        }

        return Task.WhenAll(tasks);
    }

    private Task CallService(object? serviceInstance, object notification, CancellationToken cancellationToken)
    {
        if(serviceInstance is null)
            return Task.CompletedTask;
        var handleMethod = serviceInstance.GetType().GetMethod(nameof(IMyNotificationHandler<object>.HandleAsync));
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"The service type {serviceInstance.GetType().FullName} does not have a HandleAsync method.");
        }
        var task = (Task)handleMethod.Invoke(serviceInstance, new object[] { notification, cancellationToken })!;
        return task;

    }
}
