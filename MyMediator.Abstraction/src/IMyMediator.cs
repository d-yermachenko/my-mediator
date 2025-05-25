
namespace MyMediator.Abstraction;

public interface IMyMediator
{
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
        where TResponse : class;


    Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : notnull, IRequest;

}