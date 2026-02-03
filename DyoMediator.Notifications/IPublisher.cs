using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DyoMediator.Notifications;

public interface IMyPublisher
{
    Task PublishAsync<TNotificationType>(TNotificationType notification, CancellationToken cancellationToken = default)
        where TNotificationType : class;

    Task PublishAsync<T>(IEnumerable<T> notifications, CancellationToken cancellationToken = default) where T : class  
        => Task.WhenAll(notifications.Select(notification => PublishAsync(notification, cancellationToken))) ;
}
