using AutoFixture;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Common.Enums;
using Beter.TestingTools.Generator.Application.Contracts;
using Beter.TestingTools.Generator.Application.Services.TestScenarios.MessageHandlers;
using Beter.TestingTools.Generator.UnitTests.Common;
using Moq;
using System.Text.Json.Nodes;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.TestScenarios.MessageHandlers
{
    public class FeedMessageHandlerTests
    {
        private static readonly Fixture Fixture = new();

        private readonly Mock<IPublisher> _publisher;
        private readonly Mock<IOffsetStorage> _offsetStorage;
        private readonly FeedMessageHandler _handler;

        public FeedMessageHandlerTests()
        {
            _publisher = new Mock<IPublisher>();
            _offsetStorage = new Mock<IOffsetStorage>();
            _handler = new FeedMessageHandler(_publisher.Object, _offsetStorage.Object);
        }

        [Fact]
        public void Ctor_should_has_null_guards()
        {
            AssertInjection.OfConstructor(typeof(FeedMessageHandler)).HasNullGuard();
        }

        [Fact]
        public async Task BeforePublish_UpdatesOffset_ForUpdateMessage()
        {
            // Arrange
            var matchId = "matchId";
            var playbackId = Fixture.Create<string>();
            var message = new TestScenarioMessage
            {
                Channel = ChannelNames.Incident,
                Value = JsonNode.Parse("[{\"Id\":\"matchId\",\"MsgType\":1}]")
            };

            var expectedOffset = Fixture.Create<int>();
            _offsetStorage.Setup(o => o.GetOffsetForUpdateMessage(matchId, HubKind.Incident))
                .Returns(expectedOffset);

            // Act
            await _handler.BeforePublish(message, playbackId, CancellationToken.None);

            // Assert
            Assert.Equal(expectedOffset, message.ToFeedMessages().First().Offset);
        }

        [Fact]
        public async Task BeforePublish_UpdatesOffset_ForNonUpdateMessage()
        {
            // Arrange
            var matchId = "matchId";
            var playbackId = Fixture.Create<string>();
            var message = new TestScenarioMessage
            {
                Channel = ChannelNames.Incident,
                Value = JsonNode.Parse("[{\"Id\":\"matchId\",\"MsgType\":2}]")
            };

            var expectedOffset = Fixture.Create<int>();
            _offsetStorage.Setup(o => o.GetOffsetForNonUpdateMessage(matchId, HubKind.Incident))
                .Returns(expectedOffset);

            // Act
            await _handler.BeforePublish(message, playbackId, CancellationToken.None);

            // Assert
            Assert.Equal(expectedOffset, message.ToFeedMessages().First().Offset);
        }

        [Fact]
        public async Task AfterPublish_PublishesEmptyMessage_WhenMsgTypeIsConnectionSnapshot()
        {
            // Arrange
            var playbackId = Fixture.Create<string>();
            var message = new TestScenarioMessage
            {
                MessageType = MessageTypes.Incident,
                Channel = ChannelNames.Incident,
                Value = JsonNode.Parse("[{\"MsgType\":2,\"Id\":\"1\"}]")
            };

            // Act
            await _handler.AfterPublish(message, playbackId, CancellationToken.None);

            // Assert
            _publisher.Verify(p => p.PublishEmptyAsync(MessageTypes.Incident, ChannelNames.Incident, playbackId, CancellationToken.None), Times.Once);
        }

        [Fact]
        public void IsApplicable_ReturnsTrue_ForApplicableMessageTypes()
        {
            // Arrange
            var applicableMessageTypes = new[] { MessageTypes.Scoreboard, MessageTypes.Trading, MessageTypes.Incident, MessageTypes.Timetable };

            // Act & Assert
            foreach (var messageType in applicableMessageTypes)
            {
                Assert.True(_handler.IsApplicable(messageType));
            }
        }

        [Fact]
        public void IsApplicable_ReturnsFalse_ForNonApplicableMessageType()
        {
            // Arrange
            var nonApplicableMessageType = "NonApplicableMessageType";

            // Act & Assert
            Assert.False(_handler.IsApplicable(nonApplicableMessageType));
        }
    }
}
