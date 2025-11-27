using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using DyoMediator.Abstraction;

namespace DyoMediator.Tests.Abstractions.MockImplementation;

public interface IQuestion
{
    string Question { get; set; }
}

public interface IAnswer
{
    string AdditionalInfo { get; set; }
}

public class AnswerResponse : IAnswer
{
    public string Response { get; set; } = string.Empty;
    public string AdditionalInfo { get; set; } = String.Empty;
}

public class QuestionRequest (string question) : IRequest<AnswerResponse>, IQuestion
{
    public string Question { get; set; } = question;
}



public class MockRequestHandler : IRequestHandler<QuestionRequest, AnswerResponse>
{
    public Task<AnswerResponse> HandleAsync(QuestionRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AnswerResponse { Response = $"{this.GetType().Name} gived answer to question \" {request.Question}\" "});
    }
}


public class MockRequestBehaviourOne<TRequest, TResponse> : IRequestBehaviour<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IQuestion
    where TResponse : class, IAnswer
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken = default)
    {
        var result = await next(request, cancellationToken);
        result.AdditionalInfo += $"This was decorated by {this.GetType().Name}";
        return result;
    }
}

public class MockRequestBehaviourTwo<TRequest, TResponse> : IRequestBehaviour<QuestionRequest, AnswerResponse>
    where TRequest: QuestionRequest
    where TResponse : AnswerResponse
{
    public async Task<AnswerResponse> HandleAsync(QuestionRequest request, RequestHandlerDelegate<QuestionRequest, AnswerResponse> next, CancellationToken cancellationToken = default)
    {
        var result = await next.Invoke(request, cancellationToken);
        result.Response += $"{this.GetType().Name} added after";
        result.Response = $"{this.GetType().Name} added before" + result.Response;
        return result;
    }
}



