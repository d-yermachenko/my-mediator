using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DyoMediator.Notifications;

public class MyPublisher<TNotificationBaseType>(IServiceProvider serviceProvider) : IMyPublisher<TNotificationBaseType>
    where TNotificationBaseType : class
{
    public Task PublishAsync<TMessageType>(TMessageType notification, CancellationToken cancellationToken)
        where TMessageType : class, TNotificationBaseType
    {
        Type serviceType = typeof(IMyNotificationHandler<>).MakeGenericType(notification.GetType());
        var handlers = serviceProvider.GetServices(serviceType);
        return Task.WhenAll(handlers.Select(handler => CallService(handler, notification, cancellationToken)));
    }

    private Task CallService(object serviceInstance, object notification, CancellationToken cancellationToken)
    {
        var handleMethod = serviceInstance.GetType().GetMethod(nameof(IMyNotificationHandler<>.HandleAsync));
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"The service type {serviceInstance.GetType().FullName} does not have a HandleAsync method.");
        }
        var task = (Task)handleMethod.Invoke(serviceInstance, new object[] { notification, cancellationToken })!;
        return task;

    }
}
