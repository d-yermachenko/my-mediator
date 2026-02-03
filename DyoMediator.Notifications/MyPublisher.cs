using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DyoMediator.Notifications;

public class MyPublisher(IServiceProvider serviceProvider) : IMyPublisher
{
    public Task PublishAsync<T>(T notification, CancellationToken cancellationToken = default) where T : class
    {
        // Copilot: Implement looking up all registered INotificationHandler<T> and invoking HandleAsync on each of them.
        var handlers = serviceProvider.GetServices<IMyNotificationHandler<T>>();
        return Task.WhenAll(handlers.Select(handler => handler.HandleAsync(notification, cancellationToken)));
    }
}
