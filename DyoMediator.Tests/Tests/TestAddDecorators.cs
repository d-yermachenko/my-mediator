using Microsoft.Extensions.DependencyInjection;
using DyoMediator.Tests.Abstractions.MockImplementation;
using DyoMediator.Mediator.DIExtension;
using DyoMediator.Decorators.DIExtension;
using DyoMediator.Abstraction;

namespace DyoMediator.Tests
{
    public class TestAddDecorators
    {
        [Fact]
        public async Task Request_Has_No_Decorator()
        {
            ServiceCollection services = new();
            services
                .AddMyMediator()
                .AddHandler<SumRequest, SumResponse, SumRequestHandler>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var myMediator = serviceProvider.GetRequiredService<IMyMediator>();
            SumRequest sumRequest = new(2, 2);

            var result = await myMediator.SendAsync<SumRequest, SumResponse>(sumRequest);

            Assert.NotNull(result);
            Assert.Equal(4, result.Result);

        }

        [Fact]
        public async Task Request_Have_One_Decorator()
        {
            ServiceCollection services = new();
            services
                .AddMyMediator()
                .AddHandler<SumRequest, SumResponse, SumRequestHandler>()
                .AddDecorator(typeof(MultiplyAnswerByTwoDecorator<,>));
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var myMediator = serviceProvider.GetRequiredService<IMyMediator>();
            SumRequest sumRequest = new(2, 2);

            var result = await myMediator.SendAsync<SumRequest, SumResponse>(sumRequest);

            Assert.NotNull(result);
            Assert.Equal(8, result.Result);

        }

        [Fact]
        public async Task Request_Has_Two_Decorators()
        {
            ServiceCollection services = new();
            services
                .AddMyMediator()
                .AddHandler<SumRequest, SumResponse, SumRequestHandler>()
                .AddDecorator(typeof(MultiplyAnswerByTwoDecorator<,>))
                .AddDecorator(typeof(MultiplyArgsInTwoDecorator<,>));
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var myMediator = serviceProvider.GetRequiredService<IMyMediator>();
            SumRequest sumRequest = new(2, 2);

            var result = await myMediator.SendAsync<SumRequest, SumResponse>(sumRequest);

            Assert.NotNull(result);
            Assert.Equal(16, result.Result);

        }


        [Fact]
        public async Task Request_Has_Three_Decorators()
        {
            ServiceCollection services = new();
            services
                .AddMyMediator()
                .AddHandler<SumRequest, SumResponse, SumRequestHandler>()
                .AddDecorator(typeof(MultiplyAnswerByTwoDecorator<,>))
                .AddDecorator(typeof(MultiplyArgsInTwoDecorator<,>))
                .AddDecorator(typeof(MultiplyArgumentsAndResultByTwoDecorator<,>));
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var myMediator = serviceProvider.GetRequiredService<IMyMediator>();
            SumRequest sumRequest = new(2, 2);

            var result = await myMediator.SendAsync<SumRequest, SumResponse>(sumRequest);

            Assert.NotNull(result);
            Assert.Equal(64, result.Result);

        }
    }
}
