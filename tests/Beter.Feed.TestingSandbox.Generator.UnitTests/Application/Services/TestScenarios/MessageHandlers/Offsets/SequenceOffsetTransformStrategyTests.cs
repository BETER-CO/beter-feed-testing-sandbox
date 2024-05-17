using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers.Offsets;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Moq;
using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.TestScenarios.MessageHandlers.Offsets
{
    public class SequenceOffsetTransformStrategyTests
    {
        private static readonly Fixture Fixture = new();

        private readonly Mock<IOffsetStorage> _offsetStorage;
        private readonly SequenceOffsetTransformStrategy _sequenceOffsetTransformStrategy;

        public SequenceOffsetTransformStrategyTests()
        {
            _offsetStorage = new Mock<IOffsetStorage>();
            _sequenceOffsetTransformStrategy = new SequenceOffsetTransformStrategy(_offsetStorage.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenOffsetStorageIsNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new SequenceOffsetTransformStrategy(null));
        }

        [Fact]
        public void Transform_ShouldThrowArgumentNullException_WhenMessageIsNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => _sequenceOffsetTransformStrategy.Transform(null, HubKind.Incident));
        }

        [Fact]
        public void Transform_ShouldSetOffsetForUpdateMessages()
        {
            // Arrange
            var hubKind = HubKind.Incident;
            var matchId = "matchId";
            var expectedOffset = Fixture.Create<int>();

            var message = new TestScenarioMessage
            {
                Channel = ChannelNames.Incident,
                Value = JsonNode.Parse("[{\"Id\":\"matchId\",\"MsgType\":1,\"Offset\":2}]")
            };

            _offsetStorage.Setup(o => o.GetOffsetForUpdateMessage(matchId, HubKind.Incident))
               .Returns(expectedOffset);

            // Act
            _sequenceOffsetTransformStrategy.Transform(message, hubKind);

            // Assert
            Assert.Equal(expectedOffset, message.ToFeedMessages().First().Offset);
        }

        [Fact]
        public void Transform_ShouldSetOffsetForNonUpdateMessages()
        {
            // Arrange
            var hubKind = HubKind.Incident;
            var matchId = "matchId";
            var expectedOffset = Fixture.Create<int>(); ;

            var message = new TestScenarioMessage
            {
                Channel = ChannelNames.Incident,
                Value = JsonNode.Parse("[{\"Id\":\"matchId\",\"MsgType\":3,\"Offset\":2}]")
            };

            _offsetStorage.Setup(o => o.GetOffsetForNonUpdateMessage(matchId, HubKind.Incident))
             .Returns(expectedOffset);

            // Act
            _sequenceOffsetTransformStrategy.Transform(message, hubKind);

            // Assert
            Assert.Equal(expectedOffset, message.ToFeedMessages().First().Offset);
        }
    }
}
