using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DyoMediator.Notifications;

/*public interface IMyPublisher
{
    Task PublishAsync<TNotificationType>(TNotificationType notification, CancellationToken cancellationToken = default)
        where TNotificationType : class;

    Task PublishAsync<T>(IEnumerable<T> notifications, CancellationToken cancellationToken = default) where T : class  
        => Task.WhenAll(notifications.Select(notification => PublishAsync(notification, cancellationToken))) ;
}*/

public interface IMyPublisher<in TNotificationBaseType>
    where TNotificationBaseType : notnull
{
    /// <summary>
    /// This method will publish notification to all registered handlers for this notification type.
    /// </summary>
    /// <typeparam name="TMessageType">Used to check type of notification, which has to implement TNotificationBaseType. Do not set it explicitly, just pass your notification, type will resolved by handler. Othervice, your handler can be not resolved</typeparam>
    /// <param name="notification">Notification</param>
    /// <param name="cancellationToken"></param>
    /// <remarks></remarks>
    /// <returns></returns>
    Task PublishAsync<TMessageType>(TMessageType notification, CancellationToken cancellationToken = default)
        where TMessageType : notnull, TNotificationBaseType;
}