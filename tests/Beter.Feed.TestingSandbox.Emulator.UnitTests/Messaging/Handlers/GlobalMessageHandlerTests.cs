using Moq;
using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.Messaging.Handlers
{
    public class GlobalMessageHandlerTests
    {
        private static readonly Fixture Fixture = new();

        [Fact]
        public void Constructor_NullPublisherResolver_ThrowsArgumentNullException()
        {
            // Arrange
            IFeedMessagePublisherResolver publisherResolver = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => new GlobalMessageHandler(publisherResolver));
        }

        [Fact]
        public async Task HandleAsync_NoChannelHeader_ThrowsInvalidOperationException()
        {
            // Arrange
            var publisherResolverMock = new Mock<IFeedMessagePublisherResolver>();
            var handler = new GlobalMessageHandler(publisherResolverMock.Object);

            var globalEvents = Fixture.CreateMany<GlobalMessageModel>().ToArray();
            var context = Fixture.Create<ConsumeMessageContext>();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.HandleAsync(globalEvents, context));
        }

        [Fact]
        public async Task HandleAsync_WithChannelHeader_PublishesToDefaultGroup()
        {
            // Arrange
            var applicableChannel = "applicable_channel";

            var publisherMock = new Mock<IMessagePublisher<GlobalMessageModel[]>>();
            var publisherResolverMock = new Mock<IFeedMessagePublisherResolver>();
            publisherResolverMock
                .Setup(r => r.Resolve(applicableChannel))
                .Returns(publisherMock.Object);

            var handler = new GlobalMessageHandler(publisherResolverMock.Object);

            var globalEvents = Fixture.CreateMany<GlobalMessageModel>().ToArray();
            var context = Fixture.Build<ConsumeMessageContext>()
                .With(x => x.MessageHeaders, new Dictionary<string, string>
                {
                    { HeaderNames.MessageChannel, applicableChannel }
                })
                .Create();

            // Act
            await handler.HandleAsync(globalEvents, context);

            // Assert
            publisherMock.Verify(p => p.SystemGroupPublish(GroupNames.DefaultGroupName, globalEvents, CancellationToken.None), Times.Once);
        }
    }
}
