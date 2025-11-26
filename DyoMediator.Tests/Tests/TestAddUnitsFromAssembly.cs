using Microsoft.Extensions.DependencyInjection;
using DyoMediator.Abstraction;
using DyoMediator.Tests.Abstractions.MockImplementation;
using DyoMediator.Mediator.DIExtension;
using DyoMediator.Decorators.DIExtension;

namespace DyoMediator.Tests
{
    public class TestAddUnitsFromAssembly
    {
        [Fact]
        public async Task Request_Has_Handlers_From_Assembly()
        {
            ServiceCollection services = new();
            services
                .AddMyMediator()
                .AddHandlersFromAssembly(typeof(TestAddUnitsFromAssembly).Assembly);
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            IMyMediator myMediator = serviceProvider.GetRequiredService<IMyMediator>();
            AnotherSumRequest another = new AnotherSumRequest(4, 4);

            var answer = await myMediator.SendAsync<AnotherSumRequest, AnotherSumResponse>(another);

            Assert.Equal(8, answer.Result);
        }

        [Fact]
        public async Task Request_Has_Behaviours_From_Assembly()
        {
            ServiceCollection services = new();
            services
                .AddMyMediator()
                .AddHandlersFromAssembly(typeof(TestAddUnitsFromAssembly).Assembly)
                .AddBehavioursFromAssembly(typeof(TestAddUnitsFromAssembly).Assembly);
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            IMyMediator myMediator = serviceProvider.GetRequiredService<IMyMediator>();
            SumRequest sumRequest = new SumRequest(2, 2);

            var answer = await myMediator.SendAsync<SumRequest, SumResponse>(sumRequest);

            Assert.Equal(64, answer.Result);
        }

        [Fact]
        public async Task Request_Has_Decorators_From_Assembly()
        {
            ServiceCollection services = new();
            services
                .AddMyMediator()
                .AddHandlersFromAssembly(typeof(TestAddUnitsFromAssembly).Assembly)
                .AddDecoratorsFromAssembly(typeof(TestAddUnitsFromAssembly).Assembly.GetTypes())
                
                ;
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            IMyMediator myMediator = serviceProvider.GetRequiredService<IMyMediator>();
            SumRequest sumRequest = new SumRequest(2, 2);

            var answer = await myMediator.SendAsync<SumRequest, SumResponse>(sumRequest);

            Assert.Equal(64, answer.Result);
        }

        [Fact]
        public async Task Request_Has_Decorators_and_Behaviours_From_Assembly()
        {
            ServiceCollection services = new();
            services
                .AddMyMediator()
                .AddHandlersFromAssembly(typeof(TestAddUnitsFromAssembly).Assembly)
                .AddDecoratorsFromAssembly(typeof(TestAddUnitsFromAssembly).Assembly.GetTypes())
                .AddBehavioursFromAssembly(typeof(TestAddUnitsFromAssembly).Assembly);

                ;
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            IMyMediator myMediator = serviceProvider.GetRequiredService<IMyMediator>();
            SumRequest sumRequest = new SumRequest(2, 2);

            var answer = await myMediator.SendAsync<SumRequest, SumResponse>(sumRequest);

            Assert.Equal(1024, answer.Result);
        }
    }
}
