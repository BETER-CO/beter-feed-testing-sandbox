using AutoFixture;
using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;
using Moq;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.Messaging.Handlers
{
    public class MessageHandlerResolverTests
    {
        public class TestEntity { }

        private static readonly Fixture Fixture = new();

        [Fact]
        public void Constructor_NullMessageHandlers_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<IMessageHandler> messageHandlers = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => new MessageHandlerResolver(messageHandlers));
        }

        [Fact]
        public void Resolve_NoApplicableHandler_ThrowsInvalidOperationException()
        {
            // Arrange
            var messageHandler1 = new Mock<IMessageHandler>();
            var messageHandler2 = new Mock<IMessageHandler>();
            messageHandler1.Setup(x => x.IsApplicable(It.IsAny<ConsumeMessageContext>())).Returns(false);
            messageHandler2.Setup(x => x.IsApplicable(It.IsAny<ConsumeMessageContext>())).Returns(false);
            var messageHandlers = new List<IMessageHandler>() { messageHandler1.Object, messageHandler2.Object };

            var context = Fixture.Create<ConsumeMessageContext>();
            var resolver = new MessageHandlerResolver(messageHandlers);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => resolver.Resolve(context));
        }

        [Fact]
        public void Resolve_ApplicableHandler_ReturnsHandler()
        {
            // Arrange
            var messageHandler1 = new Mock<IMessageHandler>();
            var messageHandler2 = new Mock<IMessageHandler>();
            messageHandler1.Setup(x => x.IsApplicable(It.Is<ConsumeMessageContext>(x => x.MessageType == typeof(TestEntity)))).Returns(true);
            messageHandler2.Setup(x => x.IsApplicable(It.IsAny<ConsumeMessageContext>())).Returns(false);
            var messageHandlers = new List<IMessageHandler>() { messageHandler1.Object, messageHandler2.Object };

            var resolver = new MessageHandlerResolver(messageHandlers);
            var context = Fixture.Build<ConsumeMessageContext>()
                .With(x => x.MessageType, typeof(TestEntity))
                .Create();

            // Act
            var resolvedHandler = resolver.Resolve(context);

            // Assert
            Assert.Equal(messageHandler1.Object, resolvedHandler);
        }
    }
}
