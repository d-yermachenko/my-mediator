using Microsoft.Extensions.DependencyInjection;
using MyMediator.Abstraction;

namespace MyMediator.Mediator;

file class BehaviourWrapper<TRequest, TResponse>(IRequestBehaviour<TRequest, TResponse> behaviour, IRequestHandler<TRequest, TResponse> nextRequest) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        return behaviour.HandleAsync(request, nextRequest.HandleAsync, cancellationToken);
    }
}

public class MyMediator(IServiceProvider serviceProvider) : IMyMediator
{

    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        IRequestHandler<TRequest, TResponse>? handler = serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>();
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for request type {typeof(TRequest).Name}.");
        }
        
        handler = ApplyBehaviours(handler);

        return await handler.HandleAsync(request, cancellationToken);
    }

    private IRequestHandler<TRequest, TResponse> ApplyBehaviours<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        var behaviours = GetBehaviours<TRequest, TResponse>().GetEnumerator();
        IRequestHandler<TRequest, TResponse> requestHandler = handler;
        while (behaviours.MoveNext())
             requestHandler = new BehaviourWrapper<TRequest, TResponse>(behaviours.Current, requestHandler);
        return requestHandler;
    }

    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        var requestType = request.GetType();
        var handler = serviceProvider.GetService(typeof(IRequestHandler<>).MakeGenericType(typeof(TRequest))) as IRequestHandler<TRequest>;
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for request type {typeof(TRequest).Name}.");
        }
        return handler.HandleAsync(request, cancellationToken);
    }

    private IEnumerable<IRequestBehaviour<TRequest, TResponse>> GetBehaviours<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        
        var bahaviourType = typeof(IRequestBehaviour<,>).MakeGenericType(typeof(TRequest), typeof(TResponse));
        var behaviours = serviceProvider.GetServices(bahaviourType);
        return behaviours.Cast<IRequestBehaviour<TRequest, TResponse>>();
    }

}

