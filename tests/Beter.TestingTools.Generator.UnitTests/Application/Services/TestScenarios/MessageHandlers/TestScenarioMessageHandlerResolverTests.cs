using Beter.TestingTools.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTools.Generator.Application.Services.TestScenarios.MessageHandlers;
using Beter.TestingTools.Generator.UnitTests.Common;
using Moq;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.TestScenarios.MessageHandlers
{
    public class TestScenarioMessageHandlerResolverTests
    {
        [Fact]
        public void Ctor_should_has_null_guards()
        {
            AssertInjection.OfConstructor(typeof(TestScenarioMessageHandlerResolver)).HasNullGuard();
        }

        [Fact]
        public void Resolve_WithValidMessageType_ReturnsHandler()
        {
            // Arrange
            var messageType = "MessageType";

            var mockHandler1 = new Mock<ITestScenarioMessageHandler>();
            mockHandler1.Setup(handler => handler.IsApplicable(messageType)).Returns(false);

            var mockHandler2 = new Mock<ITestScenarioMessageHandler>();
            mockHandler2.Setup(handler => handler.IsApplicable(messageType)).Returns(true);

            var resolver = new TestScenarioMessageHandlerResolver(new List<ITestScenarioMessageHandler> { mockHandler1.Object, mockHandler2.Object });

            // Act
            var result = resolver.Resolve(messageType);

            // Assert
            Assert.Equal(mockHandler2.Object, result);
        }

        [Fact]
        public void Resolve_WithInvalidMessageType_ThrowsInvalidOperationException()
        {
            // Arrange
            var messageType = "InvalidMessageType";

            var mockHandler1 = new Mock<ITestScenarioMessageHandler>();
            mockHandler1.Setup(handler => handler.IsApplicable(It.IsAny<string>())).Returns(false);

            var mockHandler2 = new Mock<ITestScenarioMessageHandler>();
            mockHandler2.Setup(handler => handler.IsApplicable(It.IsAny<string>())).Returns(false);

            var resolver = new TestScenarioMessageHandlerResolver(new List<ITestScenarioMessageHandler> { mockHandler1.Object, mockHandler2.Object });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => resolver.Resolve(messageType));
        }
    }
}
