using AutoFixture;
using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Emulator.Messaging;
using Beter.TestingTools.Emulator.Messaging.Handlers;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Models.TradingInfos;
using Moq;

namespace Beter.TestingTools.Emulator.UnitTests.Messaging.Handlers
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
