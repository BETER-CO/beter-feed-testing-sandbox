using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts;
using Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers;
using Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers.Offsets;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.UnitTests.Common;
using Moq;
using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.TestScenarios.MessageHandlers
{
    public class FeedMessageHandlerTests
    {
        private static readonly Fixture Fixture = new();

        private readonly Mock<IPublisher> _publisher;
        private readonly Mock<IOffsetTransformStrategyResolver> _offsetTransformStrategyResolver;
        private readonly FeedMessageHandler _handler;

        public FeedMessageHandlerTests()
        {
            _publisher = new Mock<IPublisher>();
            _offsetTransformStrategyResolver = new Mock<IOffsetTransformStrategyResolver>();
            _handler = new FeedMessageHandler(_publisher.Object, _offsetTransformStrategyResolver.Object);
        }

        [Fact]
        public void Ctor_should_has_null_guards()
        {
            AssertInjection.OfConstructor(typeof(FeedMessageHandler)).HasNullGuard();
        }

        [Fact]
        public async Task BeforePublish_ShouldCallTransformWithCorrectParameters()
        {
            // Arrange
            var playbackId = Fixture.Create<string>();
            var additionalInfo = Fixture.Create<AdditionalInfo>();
            var message = new TestScenarioMessage
            {
                Channel = ChannelNames.Incident,
                Value = JsonNode.Parse("[{\"Id\":\"matchId\",\"MsgType\":1,\"Offset\":2}]")
            };

            var expectedOffset = Fixture.Create<int>();
            var offsetTransformStrategy = new Mock<IOffsetTransformStrategy>();
            _offsetTransformStrategyResolver.Setup(x => x.Resolve(additionalInfo))
                .Returns(offsetTransformStrategy.Object);

            // Act
            await _handler.BeforePublish(message, playbackId, additionalInfo, CancellationToken.None);

            // Assert
            offsetTransformStrategy.Verify(o => o.Transform(message, HubKind.Incident), Times.Once);
        }

        [Fact]
        public async Task AfterPublish_PublishesEmptyMessage_WhenMsgTypeIsConnectionSnapshot()
        {
            // Arrange
            var playbackId = Fixture.Create<string>();
            var aditionalInfo = Fixture.Create<AdditionalInfo>();
            var message = new TestScenarioMessage
            {
                MessageType = MessageTypes.Incident,
                Channel = ChannelNames.Incident,
                Value = JsonNode.Parse("[{\"MsgType\":2,\"Id\":\"1\",\"Offset\":2}]")
            };

            // Act
            await _handler.AfterPublish(message, playbackId, aditionalInfo, CancellationToken.None);

            // Assert
            _publisher.Verify(p => p.PublishEmptyAsync(MessageTypes.Incident, ChannelNames.Incident, playbackId, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task AfterPublish_ShouldNotPublishEmptyAsync_WhenMessageTypeIsNotConnectionSnapshot()
        {
            // Arrange
            var playbackId = Fixture.Create<string>();
            var aditionalInfo = Fixture.Create<AdditionalInfo>();
            var message = new TestScenarioMessage
            {
                MessageType = MessageTypes.Incident,
                Channel = ChannelNames.Incident,
                Value = JsonNode.Parse("[{\"MsgType\":3,\"Id\":\"1\",\"Offset\":2}]")
            };

            // Act
            await _handler.AfterPublish(message, playbackId, aditionalInfo, CancellationToken.None);

            // Assert
            _publisher.Verify(x => x.PublishEmptyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(MessageTypes.Scoreboard, true)]
        [InlineData(MessageTypes.Trading, true)]
        [InlineData(MessageTypes.Incident, true)]
        [InlineData(MessageTypes.Timetable, true)]
        [InlineData("Unknown", false)]
        public void IsApplicable_ShouldReturnExpectedResults(string messageType, bool expected)
        {
            //Act
            var result = _handler.IsApplicable(messageType);

            //Assert
            Assert.Equal(expected, result);
        }
    }
}
