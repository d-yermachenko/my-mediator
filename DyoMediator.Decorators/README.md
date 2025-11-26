# DyoMediator.Decorators

Concrete decorators and behaviours for the mediator pipeline.

## Responsibilities
- Provides ready-to-use decorators to transform requests and responses.
- Provides behaviours to run cross-cutting logic around handler execution.

## Installation
Add a project reference or package reference to `DyoMediator.Decorators`.

## Available Examples
- `MultiplyAnswerByTwoDecorator<,>`: multiplies the numeric result by 2.
- `MultiplyArgsInTwoDecorator<,>`: multiplies input arguments by 2 before handling.
- `MultiplyArgumentsAndResultByTwoDecorator<,>`: multiplies both inputs and outputs by 2.
- `MultiplyAnswerInTwoBehaviour<,>` and `MultiplyArgsInTwoBehaviour<,>`: similar effects as behaviours.

## Registering
```csharp
using Microsoft.Extensions.DependencyInjection;
using DyoMediator.Mediator.DIExtension;
using DyoMediator.Decorators.DIExtension;

var services = new ServiceCollection();
services
    .AddMyMediator()
    .AddHandler<SumRequest, SumResponse, SumRequestHandler>()
    .AddDecorator(typeof(MultiplyAnswerByTwoDecorator<,>))
    .AddDecorator(typeof(MultiplyArgsInTwoDecorator<,>))
    .AddBehaviour(typeof(MultiplyAnswerInTwoBehaviour<,>));
```

## Custom Decorator Template
```csharp
public sealed class MyDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : class
{
    private readonly IRequestHandler<TRequest, TResponse> _inner;

    public MyDecorator(IRequestHandler<TRequest, TResponse> inner) => _inner = inner;

    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken ct = default)
    {
        // pre-processing
        var result = await _inner.HandleAsync(request, ct);
        // post-processing
        return result;
    }
}
```

## Notes
- Decorators wrap handlers and must be registered via `.AddDecorator(typeof(YourDecorator<,>))`.
- Behaviours are similar but may have different base interfaces depending on your abstraction definitions.
