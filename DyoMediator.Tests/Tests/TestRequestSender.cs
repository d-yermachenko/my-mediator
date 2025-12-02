
using Microsoft.Extensions.DependencyInjection;
using DyoMediator.Abstraction;
using DyoMediator.Mediator.DIExtension;
using DyoMediator.Tests.Abstractions.MockImplementation;

namespace DyoMediator.Tests
{
    public class TestRequestSenderDIExtension
    {
        [Fact]
        public Task DI_Container_Contains_MyMediator()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddMyMediator();
            var serviceProvider = services.BuildServiceProvider();
            // Act
            var mediator = serviceProvider.GetService<IMyMediator>();
            // Assert
            Assert.NotNull(mediator);
            return Task.CompletedTask;
        }

        [Fact]
        public async Task MyMediator_Resolves_Handler()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddMyMediator();
            services.AddHandler<QuestionRequest, AnswerResponse, MockRequestHandler>();
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMyMediator>();
            var request = new QuestionRequest("What is the capital of France?");

            // Act
            AnswerResponse? response = await mediator.SendAsync<QuestionRequest, AnswerResponse>(request, CancellationToken.None);
            // Assert
            Assert.NotNull(mediator);
            Assert.NotNull(response);
            Assert.Contains("What is the capital of France?", response?.Response);
        }
    }
}
