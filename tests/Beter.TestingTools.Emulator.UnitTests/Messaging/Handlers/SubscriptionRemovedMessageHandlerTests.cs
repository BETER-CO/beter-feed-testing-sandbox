using AutoFixture;
using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Emulator.Messaging;
using Beter.TestingTools.Emulator.Messaging.Handlers;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Models;
using Moq;

namespace Beter.TestingTools.Emulator.UnitTests.Messaging.Handlers
{
    public class SubscriptionRemovedMessageHandlerTests
    {
        private static readonly Fixture Fixture = new();

        [Fact]
        public void Constructor_NullPublisherResolver_ThrowsArgumentNullException()
        {
            // Arrange
            IFeedMessagePublisherResolver publisherResolver = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => new SubscriptionRemovedMessageHandler(publisherResolver));
        }

        [Fact]
        public async Task HandleAsync_NoChannelHeader_ThrowsInvalidOperationException()
        {
            // Arrange
            var publisherResolverMock = new Mock<IFeedMessagePublisherResolver>();
            var handler = new SubscriptionRemovedMessageHandler(publisherResolverMock.Object);

            var model = Fixture.Create<SubscriptionsRemovedModel>();
            var context = Fixture.Create<ConsumeMessageContext>();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.HandleAsync(model, context));
        }

        [Fact]
        public async Task HandleAsync_WithChannelHeader_PublishesToDefaultGroup()
        {
            // Arrange
            var applicableChannel = "applicable_channel";

            var publisherMock = new Mock<IMessagePublisher<SubscriptionsRemovedModel>>();
            var publisherResolverMock = new Mock<IFeedMessagePublisherResolver>();
            publisherResolverMock
                .Setup(r => r.Resolve(applicableChannel))
                .Returns(publisherMock.Object);

            var handler = new SubscriptionRemovedMessageHandler(publisherResolverMock.Object);
            var model = Fixture.Create<SubscriptionsRemovedModel>();
            var context = Fixture.Build<ConsumeMessageContext>()
                .With(x => x.MessageHeaders, new Dictionary<string, string>
                {
                    { HeaderNames.MessageChannel, applicableChannel }
                })
                .Create();

            // Act
            await handler.HandleAsync(model, context);

            // Assert
            publisherMock.Verify(p => p.SendGroupRemoveSubscriptionsAsync(GroupNames.DefaultGroupName, model.Ids.ToArray(), CancellationToken.None), Times.Once);
        }
    }
}
