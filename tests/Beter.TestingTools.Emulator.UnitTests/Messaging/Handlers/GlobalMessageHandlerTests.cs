using Beter.TestingTools.Emulator.Messaging.Handlers;
using Beter.TestingTools.Emulator.Messaging;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Models.GlobalEvents;
using Moq;
using Beter.TestingTools.Common.Constants;
using AutoFixture;

namespace Beter.TestingTools.Emulator.UnitTests.Messaging.Handlers
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
