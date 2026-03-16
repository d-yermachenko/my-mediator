namespace DyoMediator.Notifications;

public interface IMyNotificationHandler<in T> where T: notnull
{
    Task HandleAsync(T notification, CancellationToken cancellationToken = default);

}
