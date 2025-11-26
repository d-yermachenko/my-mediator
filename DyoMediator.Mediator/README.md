# DyoMediator.Mediator

Core mediator implementation and DI extensions to register handlers, decorators, and behaviours.

## Responsibilities
- Implements `IMyMediator` request dispatching.
- Composes pipeline of handlers, decorators, and behaviours.
- Provides DI extensions to register components.

## Installation
Add a project reference or package reference to `DyoMediator.Mediator`.

## Quick Start
```csharp
using Microsoft.Extensions.DependencyInjection;
using DyoMediator.Abstraction;
using DyoMediator.Mediator.DIExtension;

var services = new ServiceCollection();
services
    .AddMyMediator() // registers IMyMediator
    .AddHandler<MyRequest, MyResponse, MyRequestHandler>();

var sp = services.BuildServiceProvider();
var mediator = sp.GetRequiredService<IMyMediator>();

var response = await mediator.SendAsync<MyRequest, MyResponse>(new MyRequest(/* ... */));
```

## Registering Decorators and Behaviours
Decorators wrap handler calls and can modify request/response. Behaviours are similar pipeline components.
```csharp
services
    .AddMyMediator()
    .AddHandler<SumRequest, SumResponse, SumRequestHandler>()
    .AddDecorator(typeof(MultiplyAnswerByTwoDecorator<,>))
    .AddBehaviour(typeof(MultiplyAnswerInTwoBehaviour<,>));
```

## Handler Example
```csharp
public sealed class SumRequest : IRequest<SumResponse>
{
    public int A { get; }
    public int B { get; }
    public SumRequest(int a, int b) { A = a; B = b; }
}

public sealed class SumResponse { public int Result { get; init; } }

public sealed class SumRequestHandler : IRequestHandler<SumRequest, SumResponse>
{
    public Task<SumResponse> HandleAsync(SumRequest request, CancellationToken ct = default)
        => Task.FromResult(new SumResponse { Result = request.A + request.B });
}
```

## Notes
- Use `AddHandler<TReq,TRes,THandler>()` to register a handler.
- Use `AddDecorator(typeof(YourDecorator<,>))` and `AddBehaviour(typeof(YourBehaviour<,>))` to extend the pipeline.
