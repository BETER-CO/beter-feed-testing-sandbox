using Beter.TestingTools.Emulator.Messaging.Handlers.Abstract;
using Beter.TestingTools.Emulator.Messaging;

namespace Beter.TestingTools.Emulator.UnitTests.Messaging.Handlers.Abstract
{
    public class MessageHandlerBaseTests
    {
        [Fact]
        public async Task HandleAsync_CallsConcreteHandler()
        {
            // Arrange
            var handler = new TestMessageHandler();
            var context = new ConsumeMessageContext
            {
                MessageObject = "TestValue",
                MessageType = typeof(string)
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await handler.HandleAsync(context, cancellationToken);

            // Assert
            Assert.True(handler.HandleAsyncCalled);
        }

        [Fact]
        public void IsApplicable_ReturnsTrueForMatchingMessageType()
        {
            // Arrange
            var handler = new TestMessageHandler();
            var context = new ConsumeMessageContext
            {
                MessageType = typeof(string)
            };

            // Act
            var isApplicable = handler.IsApplicable(context);

            // Assert
            Assert.True(isApplicable);
        }

        [Fact]
        public void IsApplicable_ReturnsFalseForNonMatchingMessageType()
        {
            // Arrange
            var handler = new TestMessageHandler();
            var context = new ConsumeMessageContext
            {
                MessageType = typeof(int)
            };

            // Act
            var isApplicable = handler.IsApplicable(context);

            // Assert
            Assert.False(isApplicable);
        }
    }

    public class TestMessageHandler : MessageHandlerBase<string>
    {
        public bool HandleAsyncCalled { get; private set; }

        public override Task HandleAsync(string value, ConsumeMessageContext context, CancellationToken cancellationToken)
        {
            HandleAsyncCalled = true;
            return Task.CompletedTask;
        }
    }
}
