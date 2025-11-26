# DyoMediator.Abstraction

Common abstractions for the mediator pattern used by all packages.

## Responsibilities
- Defines `IMyMediator` interface for sending requests.
- Defines `IRequest<TResponse>` and related handler/behaviour/decorator contracts.
- Serves as minimal dependency shared across projects.

## Installation
Add a project reference or package reference to `DyoMediator.Abstraction`.

## Key Interfaces
```csharp
public interface IMyMediator
{
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
        where TResponse : class;

    Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : notnull, IRequest;
}

public interface IRequest<TResponse> { }

public interface IRequestHandler<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
```

## Example Request/Response
```csharp
public sealed class QuestionRequest : IRequest<AnswerResponse>
{
    public string Question { get; }
    public QuestionRequest(string question) => Question = question;
}

public sealed class AnswerResponse
{
    public string Response { get; init; } = string.Empty;
}
```

## Usage with Mediator
See `DyoMediator.Mediator` README for DI and pipeline registration.
