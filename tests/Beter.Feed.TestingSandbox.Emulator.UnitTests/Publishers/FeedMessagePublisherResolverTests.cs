using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Moq;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.Publishers
{
    public class FeedMessagePublisherResolverTests
    {
        [Theory]
        [InlineData(ChannelNames.Scoreboard, HubKind.Scoreboard)]
        [InlineData(ChannelNames.Timetable, HubKind.TimeTable)]
        [InlineData(ChannelNames.Incident, HubKind.Incident)]
        [InlineData(ChannelNames.Trading, HubKind.Trading)]
        public void Resolve_WithValidChannel_ReturnsCorrectPublisher(string channelName, HubKind hubKind)
        {
            // Arrange
            var publisher = new Mock<IMessagePublisher>();
            publisher.Setup(x => x.Hub).Returns(hubKind);

            var resolver = new FeedMessagePublisherResolver(new List<IMessagePublisher>() { publisher.Object });

            // Act
            var resolvedPublisher = resolver.Resolve(channelName);

            // Assert
            Assert.NotNull(resolvedPublisher);
        }

        [Fact]
        public void Resolve_WithNullOrEmptyChannel_ThrowsArgumentException()
        {
            //Arrange
            var resolver = new FeedMessagePublisherResolver(Enumerable.Empty<IMessagePublisher>());

            // Act & Assert
            Assert.Throws<ArgumentException>(() => resolver.Resolve(null));
            Assert.Throws<ArgumentException>(() => resolver.Resolve(string.Empty));
        }

        [Fact]
        public void Resolve_WithUnsupportedChannel_ThrowsInvalidOperationException()
        {
            // Arrange
            var channel = "UnsupportedChannel";
            var resolver = new FeedMessagePublisherResolver(Enumerable.Empty<IMessagePublisher>());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => resolver.Resolve(channel));
        }
    }
}
