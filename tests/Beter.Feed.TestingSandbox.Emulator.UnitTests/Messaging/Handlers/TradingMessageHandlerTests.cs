using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Moq;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.Messaging.Handlers
{
    public class TradingMessageHandlerTests
    {
        private static readonly Fixture Fixture = new();

        [Fact]
        public void Constructor_NullPublisher_ThrowsArgumentNullException()
        {
            // Arrange
            IMessagePublisher<TradingInfoModel> publisher = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => new TradingMessageHandler(publisher));
        }

        [Fact]
        public async Task HandleAsync_SendsMessageToPublisher()
        {
            // Arrange
            var publisher = new Mock<IMessagePublisher<TradingInfoModel>>();
            var handler = new TradingMessageHandler(publisher.Object);

            var messages = Fixture.CreateMany<TradingInfoModel>().ToArray();
            var context = Fixture.Create<ConsumeMessageContext>();

            // Act
            await handler.HandleAsync(messages, context, CancellationToken.None);

            // Assert
            publisher.Verify(p => p.GroupPublish(GroupNames.DefaultGroupName, messages, CancellationToken.None), Times.Once);
        }
    }
}
