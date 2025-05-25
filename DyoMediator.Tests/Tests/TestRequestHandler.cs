using DyoMediator.Tests.Abstractions.MockImplementation;
using Xunit;
namespace DyoMediator.Tests
{
    public class TestRequestHandler
    {
        [Fact]
        public async Task RequestAnswer_Contains_HandlerName()
        {
            // Arrange
            var request = new QuestionRequest("What is the capital of France?");
            var handler = new MockRequestHandler();
            var expectedResponse = $"{handler.GetType().Name} gived answer to question \" {request.Question}\" ";
            // Act
            var response = await handler.HandleAsync(request);
            // Assert
            Assert.Equal(expectedResponse, response.Response);
        }

        [Fact]
        public async Task RequestAnswer_Contains_BehaviourTwoName()
        {
            // Arrange
            var request = new QuestionRequest("What is the capital of France?");
            var handler = new MockRequestHandler();
            var behaviour = new MockRequestBehaviourTwo<QuestionRequest, AnswerResponce>();
            var expectedResponse = $"{behaviour.GetType().Name}";
            // Act
            var response = await behaviour.HandleAsync(request, handler.HandleAsync);
            // Assert
            Assert.Contains(expectedResponse, response.Response);
        }


    }
}
