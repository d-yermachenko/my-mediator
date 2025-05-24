using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using MyMediator.Abstraction;

namespace MyMediator.Tests.Abstractions.MockImplementation;

public interface IQuestion
{
    string Question { get; set; }
}

public interface IAnswer
{
    string AdditionalInfo { get; set; }
}

public class AnswerResponce : IAnswer
{
    public string Response { get; set; } = string.Empty;
    public string AdditionalInfo { get; set; } = String.Empty;
}

public class QuestionRequest (string question) : IRequest<AnswerResponce>, IQuestion
{
    public string Question { get; set; } = question;
}



public class MockRequestHandler : IRequestHandler<QuestionRequest, AnswerResponce>
{
    public Task<AnswerResponce> HandleAsync(QuestionRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AnswerResponce { Response = $"{this.GetType().Name} gived answer to question \" {request.Question}\" "});
    }
}


public class MockRequestBehaviourOne<TRequest, TResponce> : IRequestBehaviour<TRequest, TResponce>
    where TRequest : IRequest<TResponce>, IQuestion
    where TResponce : class, IAnswer
{
    public async Task<TResponce> HandleAsync(TRequest request, RequestHandlerDelegate<TRequest, TResponce> next, CancellationToken cancellationToken = default)
    {
        var result = await next(request, cancellationToken);
        result.AdditionalInfo += $"This was gecorated by {this.GetType().Name}";
        return result;
    }
}

public class MockRequestBehaviourTwo<TRequest, TResponce> : IRequestBehaviour<QuestionRequest, AnswerResponce>
    where TRequest: QuestionRequest
    where TResponce : AnswerResponce
{
    public async Task<AnswerResponce> HandleAsync(QuestionRequest request, RequestHandlerDelegate<QuestionRequest, AnswerResponce> next, CancellationToken cancellationToken = default)
    {
        var result = await next.Invoke(request, cancellationToken);
        result.Response += $"{this.GetType().Name} added after";
        result.Response = $"{this.GetType().Name} added before" + result.Response;
        return result;
    }
}



