namespace DyoMediator.Abstraction;

public delegate Task<TResponse> RequestHandlerDelegate<in TRequest, TResponse>(TRequest next, CancellationToken cancellationToken = default)
    where TRequest : IRequest<TResponse>;

public delegate Task RequestHandlerDelegate<in TRequest>(TRequest next, CancellationToken cancellationToken = default)
    where TRequest : IRequest;

public interface IRequest;


public interface IRequest<out TResponse> : IRequest;

public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IRequestHandler<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request and returns a response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <returns>The response from handling the request.</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}


public interface IRequestBehaviour<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request and returns a response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <returns>The response from handling the request.</returns>
    Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken = default);
}

public interface IRequestBehaviour<TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Handles the request and returns a response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <returns>The response from handling the request.</returns>
    Task HandleAsync(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken = default);
}






