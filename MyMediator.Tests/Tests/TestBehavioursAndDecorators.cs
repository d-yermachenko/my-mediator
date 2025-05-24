using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MyMediator.Abstraction;
using MyMediator.Tests.Abstractions.MockImplementation;
using MyMediator.Mediator.DIExtension;
using MyMediator.Decorators.DIExtension;

namespace MyMediator.Tests.Tests;

public class TestBehavioursAndDecorators
{
    [Fact]
    public async Task Request_Has_Behaviour_And_Decorator()
    {
        ServiceCollection services = new();
        services
            .AddMyMediator()
            .AddHandler<SumRequest, SumResponse, SumRequestHandler>()
            //.AddDecorator(typeof(MultiplyAnswerByTwoDecorator<,>))
            .AddDecorator(typeof(MultiplyArgsInTwoDecorator<,>))
            //.AddDecorator(typeof(MultiplyArgumentsAndResultByTwoDecorator<,>))
            //.AddBehaviour(typeof(MultiplyArgsInTwoBehaviour<,>))
            .AddBehaviour(typeof(MultiplyAnswerInTwoBehaviour<,>))
            //.AddBehaviour(typeof(MultiplyResultAndArgsInTwoBehaviour<,>))
            ; 
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        var myMediator = serviceProvider.GetRequiredService<IMyMediator>();
        SumRequest sumRequest = new(2, 2);

        var result = await myMediator.SendAsync<SumRequest, SumResponse>(sumRequest);

        Assert.NotNull(result);
        Assert.Equal(16, result.Result);

    }

    [Fact]
    public async Task Request_Has_Two_Behaviour_And_Two_Decorator()
    {
        ServiceCollection services = new();
        services
            .AddMyMediator()
            .AddHandler<SumRequest, SumResponse, SumRequestHandler>()
            .AddDecorator(typeof(MultiplyAnswerByTwoDecorator<,>))
            .AddDecorator(typeof(MultiplyArgsInTwoDecorator<,>))
            //.AddDecorator(typeof(MultiplyArgumentsAndResultByTwoDecorator<,>))
            .AddBehaviour(typeof(MultiplyArgsInTwoBehaviour<,>))
            .AddBehaviour(typeof(MultiplyAnswerInTwoBehaviour<,>))
            //.AddBehaviour(typeof(MultiplyResultAndArgsInTwoBehaviour<,>))
            ;
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        var myMediator = serviceProvider.GetRequiredService<IMyMediator>();
        SumRequest sumRequest = new(2, 2);

        var result = await myMediator.SendAsync<SumRequest, SumResponse>(sumRequest);

        Assert.NotNull(result);
        Assert.Equal(64, result.Result);

    }
}
