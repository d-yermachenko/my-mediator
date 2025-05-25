using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DyoMediator.Abstraction;

namespace DyoMediator.Tests.Abstractions.MockImplementation;

public interface ITwoNumbersRequest
{
    int NumberOne { get; init; }

    ITwoNumbersRequest AlterNumberOne(Func<int, int> alter);

    int NumberTwo { get; init; }

    ITwoNumbersRequest AlterNumberTwo(Func<int, int> alter);
}

public interface ITwoNumberResult
{
    double Result{ get; }

    ITwoNumberResult AlterResult(Func<double, double> alter);
}

#region Sum Query

public record SumRequest(int NumberOne, int NumberTwo) : IRequest<SumResponse>, ITwoNumbersRequest
{
    public ITwoNumbersRequest AlterNumberOne(Func<int, int> alter) => this with { NumberOne = alter?.Invoke(NumberOne) ?? NumberOne };

    public ITwoNumbersRequest AlterNumberTwo(Func<int, int> alter) => this with { NumberTwo = alter?.Invoke(NumberTwo) ?? NumberTwo };
}

public record SumResponse(double Result) : ITwoNumberResult
{
    public ITwoNumberResult AlterResult(Func<double, double> alter) => this with { Result = alter?.Invoke(Result) ?? Result };

}

public class SumRequestHandler : IRequestHandler<SumRequest, SumResponse>
{
    public Task<SumResponse> HandleAsync(SumRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SumResponse(request.NumberOne + request.NumberTwo) );
    }
}
#endregion

#region Difference Query
public record DifRequest(int NumberOne, int NumberTwo) : IRequest<DifResponse>, ITwoNumbersRequest
{
    public ITwoNumbersRequest AlterNumberOne(Func<int, int> alter) => this with { NumberOne = alter?.Invoke(NumberOne) ?? NumberOne };

    public ITwoNumbersRequest AlterNumberTwo(Func<int, int> alter) => this with { NumberTwo = alter?.Invoke(NumberTwo) ?? NumberTwo };
}

public record DifResponse(double Result) : ITwoNumberResult
{
    public ITwoNumberResult AlterResult(Func<double, double> alter) => this with { Result = alter?.Invoke(Result) ?? Result };
}

public class DifRequestHandler : IRequestHandler<DifRequest, DifResponse>
{
    public Task<DifResponse> HandleAsync(DifRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new DifResponse(request.NumberOne + request.NumberTwo));
    }
}

#endregion

#region Another Sum Query

public record AnotherSumRequest(int NumberOne, int NumberTwo) : IRequest<AnotherSumResponse>;

public record AnotherSumResponse(double Result) ;


public class AnotherSumRequestHandler : IRequestHandler<AnotherSumRequest, AnotherSumResponse>
{
    public Task<AnotherSumResponse> HandleAsync(AnotherSumRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AnotherSumResponse(request.NumberOne + request.NumberTwo));
    }
}
#endregion

#region Two number behaviours

public class MultiplyArgsInTwoBehaviour<TRequest, TResponse> : IRequestBehaviour<TRequest, TResponse>
    where TRequest : IRequest<TResponse> 
{
    public Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken = default)
    {
        if(request is ITwoNumbersRequest r)
            request = (TRequest)r.AlterNumberOne(x => x * 2).AlterNumberTwo(x => x * 2);
        return next(request, cancellationToken);
    }
}

public class MultiplyAnswerInTwoBehaviour<TRequest, TResponse> : IRequestBehaviour<TRequest, TResponse>
    where TRequest : IRequest<TResponse>

{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken = default)
    {
        var result = await next(request) ;
        if(result is ITwoNumberResult tnr)
            result = (TResponse)tnr.AlterResult(x => x * 2);
        return result;
    }
}

public class MultiplyResultAndArgsInTwoBehaviour<TRequest, TResponse> : IRequestBehaviour<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken = default)
    {
        if (request is ITwoNumbersRequest r)
            request = (TRequest)r.AlterNumberOne(x => x * 2).AlterNumberTwo(x => x * 2);
        var result = await next(request);
        if (result is ITwoNumberResult tnr)
            result = (TResponse)tnr.AlterResult(x => x * 2);

        return result;
    }
}

#endregion

#region Two number Decorators

public class MultiplyAnswerByTwoDecorator<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> Next) : IRequestDecorator<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var result = await Next.HandleAsync(request);
        if (result is ITwoNumberResult tnr)
            result = (TResponse)tnr.AlterResult(x => x * 2);
        return result;
    }
}

public class MultiplyArgsInTwoDecorator<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> Next) : IRequestDecorator<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        if (request is ITwoNumbersRequest r)
            request = (TRequest)r.AlterNumberOne(x => x * 2).AlterNumberTwo(x => x * 2);
        return await Next.HandleAsync(request, cancellationToken);
    }
}

public class MultiplyArgumentsAndResultByTwoDecorator<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> Next) : IRequestDecorator<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        if (request is ITwoNumbersRequest r)
            request = (TRequest)r.AlterNumberOne(x => x * 2).AlterNumberTwo(x => x * 2);
        var result = await Next.HandleAsync(request);
        if (result is ITwoNumberResult tnr)
            result = (TResponse)tnr.AlterResult(x => x * 2);

        return result;
    }
}

#endregion