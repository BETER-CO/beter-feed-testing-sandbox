using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.Messaging
{
    public class GeneratorMessagesListenerTests
    {
        private readonly Mock<IGeneratorMessagesConsumer> _consumer;
        private readonly GeneratorMessagesListener _listener;

        public GeneratorMessagesListenerTests()
        {
            _consumer = new Mock<IGeneratorMessagesConsumer>();
            _listener = new GeneratorMessagesListener(_consumer.Object, new NullLogger<GeneratorMessagesListener>());
        }

        [Fact]
        public async Task StartAsync_StartsConsumer()
        {
            // Arrange - ExecuteAsync runs as a background task, so it is not guaranteed to have
            // reached the consumer by the time StartAsync returns. Wait for the call instead.
            var consuming = new TaskCompletionSource();

            _consumer.Setup(consumer => consumer.StartConsuming(It.IsAny<CancellationToken>()))
                .Callback(() => consuming.TrySetResult())
                .Returns(Task.CompletedTask);

            // Act
            await _listener.StartAsync(CancellationToken.None);

            // Assert
            var finished = await Task.WhenAny(consuming.Task, Task.Delay(TimeSpan.FromSeconds(10)));

            Assert.Same(consuming.Task, finished);
            _consumer.Verify(consumer => consumer.StartConsuming(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Dispose_CallsConsumerDispose()
        {
            // Act
            _listener.Dispose();

            // Assert
            _consumer.Verify(consumer => consumer.Dispose(), Times.Once);
        }
    }
}
