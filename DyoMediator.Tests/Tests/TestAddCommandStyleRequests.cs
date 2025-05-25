using Microsoft.Extensions.DependencyInjection;
using DyoMediator.Abstraction;
using DyoMediator.Tests.Abstractions.MockImplementation;
using DyoMediator.Mediator.DIExtension;
using DyoMediator.Decorators.DIExtension;

namespace DyoMediator.Tests;

public class TestAddCommandStyleRequests
{

    [Fact]
    public async Task Command_Executes_Solo()
    {
        ServiceCollection services = new();
        services
            .AddMyMediator()
            .AddHandler<FooWritingCommand, FooWritingCommandHandler>()
            .AddSingleton<ISomeExternalService, SomeExternalService>()
            ;
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        var myMediator = serviceProvider.GetRequiredService<IMyMediator>();
        FooWritingCommand fooWritingCommand = new("Here we writing foo");
        ISomeExternalService someExternalService = serviceProvider.GetRequiredService<ISomeExternalService>();

        await myMediator.SendAsync(fooWritingCommand);

        Assert.NotEmpty(someExternalService.GetValue(fooWritingCommand.Key + "-Handler"));
        Assert.Equal(someExternalService.GetValue(fooWritingCommand.Key + "-Handler"), fooWritingCommand.Value);
    }

    [Fact]
    public async Task Command_Executes_With_Decorator()
    {
        ServiceCollection services = new();
        services
            .AddMyMediator()
            .AddHandler<FooWritingCommand, FooWritingCommandHandler>()
            .AddSingleton<ISomeExternalService, SomeExternalService>()
            .AddDecorator(typeof(StorageServiceDecorator<>))
            //.AddDecorator(typeof(MultiplyAnswerByTwoDecorator<,>))
            //.AddDecorator(typeof(MultiplyArgsInTwoDecorator<,>))
            //.AddDecorator(typeof(MultiplyArgumentsAndResultByTwoDecorator<,>))
            //.AddBehaviour(typeof(MultiplyArgsInTwoBehaviour<,>))
            //.AddBehaviour(typeof(MultiplyAnswerInTwoBehaviour<,>))
            //.AddBehaviour(typeof(MultiplyResultAndArgsInTwoBehaviour<,>))
            ;
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        var myMediator = serviceProvider.GetRequiredService<IMyMediator>();
        FooWritingCommand fooWritingCommand = new("Here we writing foo");
        ISomeExternalService someExternalService = serviceProvider.GetRequiredService<ISomeExternalService>();

        await myMediator.SendAsync(fooWritingCommand);

        Assert.NotEmpty(someExternalService.GetValue(fooWritingCommand.Key + "-Handler"));
        Assert.Equal(someExternalService.GetValue(fooWritingCommand.Key + "-Handler"), fooWritingCommand.Value);
        Assert.Contains(someExternalService.GetValue(fooWritingCommand.Key + "-Decorator"), "DECORATOR");
    }

    [Fact]
    public async Task Command_Executes_With_Behaviour()
    {
        ServiceCollection services = new();
        services
            .AddMyMediator()
            .AddHandler<FooWritingCommand, FooWritingCommandHandler>()
            .AddSingleton<ISomeExternalService, SomeExternalService>()
            .AddBehaviour(typeof(StorageServiceBehaviour<>))
            
            //.AddDecorator(typeof(MultiplyAnswerByTwoDecorator<,>))
            //.AddDecorator(typeof(MultiplyArgsInTwoDecorator<,>))
            //.AddDecorator(typeof(MultiplyArgumentsAndResultByTwoDecorator<,>))
            //.AddBehaviour(typeof(MultiplyArgsInTwoBehaviour<,>))
            //.AddBehaviour(typeof(MultiplyAnswerInTwoBehaviour<,>))
            //.AddBehaviour(typeof(MultiplyResultAndArgsInTwoBehaviour<,>))
            ;
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        var myMediator = serviceProvider.GetRequiredService<IMyMediator>();
        FooWritingCommand fooWritingCommand = new("Here we writing foo");
        ISomeExternalService someExternalService = serviceProvider.GetRequiredService<ISomeExternalService>();

        await myMediator.SendAsync(fooWritingCommand);

        Assert.NotEmpty(someExternalService.GetValue(fooWritingCommand.Key + "-Handler"));
        Assert.Equal(someExternalService.GetValue(fooWritingCommand.Key + "-Handler"), fooWritingCommand.Value);
        Assert.Contains(someExternalService.GetValue(fooWritingCommand.Key + "-Behaviour"), "BEHAVIOUR");
    }

    [Fact]
    public async Task Command_Executes_With_Behaviour_And_Decorator()
    {
        ServiceCollection services = new();
        services
            .AddMyMediator()
            .AddHandler<FooWritingCommand, FooWritingCommandHandler>()
            .AddSingleton<ISomeExternalService, SomeExternalService>()
            .AddBehaviour(typeof(StorageServiceBehaviour<>))
            .AddDecorator(typeof(StorageServiceDecorator<>))
            //.AddDecorator(typeof(MultiplyAnswerByTwoDecorator<,>))
            //.AddDecorator(typeof(MultiplyArgsInTwoDecorator<,>))
            //.AddDecorator(typeof(MultiplyArgumentsAndResultByTwoDecorator<,>))
            //.AddBehaviour(typeof(MultiplyArgsInTwoBehaviour<,>))
            //.AddBehaviour(typeof(MultiplyAnswerInTwoBehaviour<,>))
            //.AddBehaviour(typeof(MultiplyResultAndArgsInTwoBehaviour<,>))
            ;
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        var myMediator = serviceProvider.GetRequiredService<IMyMediator>();
        FooWritingCommand fooWritingCommand = new("Here we writing foo");
        ISomeExternalService someExternalService = serviceProvider.GetRequiredService<ISomeExternalService>();

        await myMediator.SendAsync(fooWritingCommand);

        Assert.NotEmpty(someExternalService.GetValue(fooWritingCommand.Key + "-Handler"));
        Assert.Equal(someExternalService.GetValue(fooWritingCommand.Key + "-Handler"), fooWritingCommand.Value);
        Assert.Contains(someExternalService.GetValue(fooWritingCommand.Key + "-Behaviour"), "BEHAVIOUR");
        Assert.Contains(someExternalService.GetValue(fooWritingCommand.Key + "-Decorator"), "DECORATOR");
    }


}
