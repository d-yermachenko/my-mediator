using Microsoft.Extensions.DependencyInjection;
using MyMediator.Abstraction;
using MyMediator.Mediator.DIExtension;
using MyMediator.Tests.Abstractions.MockImplementation;

namespace MyMediator.Tests.Tests;

public class TestAddBehaviours
{
    [Fact]
    public async Task Request_Has_Behaviours()
    {
        ServiceCollection services = new();
        services
            .AddMyMediator()
            .AddHandler<SumRequest, SumResponse, SumRequestHandler>()
            .AddHandler<DifRequest, DifResponse, DifRequestHandler>()
            .AddHandler<AnotherSumRequest, AnotherSumResponse, AnotherSumRequestHandler>()
            .AddScoped(typeof(IRequestBehaviour<,>), typeof(MultiplyArgsInTwoBehaviour<,>))
            .AddScoped(typeof(IRequestBehaviour<,>), typeof(MultiplyAnswerInTwoBehaviour<,>))
            .AddScoped(typeof(IRequestBehaviour<,>), typeof(MultiplyResultAndArgsInTwoBehaviour<,>));
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        IMyMediator myMediator = serviceProvider.GetRequiredService<IMyMediator>();
        SumRequest sumRequest = new(2, 2);

        SumResponse? response = await myMediator.SendAsync<SumRequest, SumResponse>(sumRequest); 

        Assert.NotNull(response);
        Assert.Equal(64, response.Result);
    }


    [Fact]
    public async Task Can_Use_AddBehaviour()
    {
        ServiceCollection services = new();
        services
            .AddMyMediator()
            .AddHandler<SumRequest, SumResponse, SumRequestHandler>()
            .AddHandler<DifRequest, DifResponse, DifRequestHandler>()
            .AddHandler<AnotherSumRequest, AnotherSumResponse, AnotherSumRequestHandler>()
            .AddBehaviour(typeof(MultiplyArgsInTwoBehaviour<,>))
            .AddBehaviour(typeof(MultiplyAnswerInTwoBehaviour<,>))
            .AddBehaviour(typeof(MultiplyResultAndArgsInTwoBehaviour<,>));

        IServiceProvider serviceProvider = services.BuildServiceProvider();
        IMyMediator myMediator = serviceProvider.GetRequiredService<IMyMediator>();
        SumRequest sumRequest = new(2, 2);

        SumResponse? response = await myMediator.SendAsync<SumRequest, SumResponse>(sumRequest);

        Assert.NotNull(response);
        Assert.Equal(64, response.Result);
    }
}
