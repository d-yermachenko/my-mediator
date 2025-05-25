namespace DyoMediator.Abstraction;

public interface IRequestDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>;


public interface IRequestDecorator<TRequest> : IRequestHandler<TRequest>
    where TRequest : IRequest;

/*public interface IRequestDecorator<in TRequest>
    where TRequest : IRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IRequestDecorator<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request and returns a response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <returns>The response from handling the request.</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}*/



