using Microsoft.Extensions.DependencyInjection;
using DyoMediator.Abstraction;
using DyoMediator.Mediator.DIExtension;
using DyoMediator.Tests.Abstractions.MockImplementation;
using DyoMediator.Decorators.DIExtension;

namespace DyoMediator.Tests;

public class TestHeterogenousPlots
{
    [Fact]
    public async Task Command_Executes_With_Behaviour_And_Decorator()
    {
        //Arrange
        ServiceCollection services = new();
        services
            .AddMyMediator()
            .AddHandler<FooWritingCommand, FooWritingCommandHandler>()
            .AddSingleton<ISomeExternalService, SomeExternalService>()
            .AddHandler<SumRequest, SumResponse, SumRequestHandler>()
            .AddHandler<DifRequest, DifResponse, DifRequestHandler>()
            .AddHandler<AnotherSumRequest, AnotherSumResponse, AnotherSumRequestHandler>()
            .AddBehaviour(typeof(StorageServiceBehaviour<>))
            .AddDecorator(typeof(StorageServiceDecorator<>))
            .AddDecorator(typeof(MultiplyArgumentsAndResultByTwoDecorator<,>))
            .AddBehaviour(typeof(MultiplyArgsInTwoBehaviour<,>))
            ;
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        var myMediator = serviceProvider.GetRequiredService<IMyMediator>();
        FooWritingCommand fooWritingCommand = new("Here we writing foo");
        SumRequest sum = new(2, 2);
        AnotherSumRequest anotherSum = new(2, 2);
        ISomeExternalService someExternalService = serviceProvider.GetRequiredService<ISomeExternalService>();

        //Act
        await myMediator.SendAsync(fooWritingCommand);
        var sumResult = await myMediator.SendAsync<SumRequest, SumResponse>(sum);
        var anotherSumResult = await myMediator.SendAsync<AnotherSumRequest, AnotherSumResponse>(anotherSum);

        //Assert
        Assert.NotEmpty(someExternalService.GetValue(fooWritingCommand.Key + "-Handler"));
        Assert.Equal(someExternalService.GetValue(fooWritingCommand.Key + "-Handler"), fooWritingCommand.Value);
        Assert.Contains(someExternalService.GetValue(fooWritingCommand.Key + "-Behaviour"), "BEHAVIOUR");
        Assert.Contains(someExternalService.GetValue(fooWritingCommand.Key + "-Decorator"), "DECORATOR");
        Assert.Equal(32, sumResult.Result);
        Assert.Equal(4, anotherSumResult.Result);

    }
}
