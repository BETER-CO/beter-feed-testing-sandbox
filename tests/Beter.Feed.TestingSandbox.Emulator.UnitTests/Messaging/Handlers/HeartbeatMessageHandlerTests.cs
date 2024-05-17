using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Beter.Feed.TestingSandbox.Models;
using Moq;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.Messaging.Handlers
{
    public class HeartbeatMessageHandlerTests
    {
        private static readonly Fixture Fixture = new();

        [Fact]
        public void Constructor_NullPublishers_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<IMessagePublisher> publishers = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => new HeartbeatMessageHandler(publishers));
        }

        [Fact]
        public async Task HandleAsync_SendsMessageToAllPublishers()
        {
            // Arrange
            var mockPublisher1 = new Mock<IMessagePublisher>();
            var mockPublisher2 = new Mock<IMessagePublisher>();

            var handler = new HeartbeatMessageHandler(new[] { mockPublisher1.Object, mockPublisher2.Object });

            var message = Fixture.Create<HeartbeatModel>();
            var context = Fixture.Create<ConsumeMessageContext>();

            // Act
            await handler.HandleAsync(message, context, CancellationToken.None);

            // Assert
            mockPublisher1.Verify(p => p.SendGroupOnHeartbeatAsync(GroupNames.DefaultGroupName, CancellationToken.None), Times.Once);
            mockPublisher2.Verify(p => p.SendGroupOnHeartbeatAsync(GroupNames.DefaultGroupName, CancellationToken.None), Times.Once);
        }
    }
}
