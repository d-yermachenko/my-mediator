namespace DyoMediator.Notifications;

public interface IMyNotificationHandler<in T> where T: class
{
    Task HandleAsync(T notification, CancellationToken cancellationToken = default);

}
