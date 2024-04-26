using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Helpers;
using Beter.TestingTools.Models.GlobalEvents;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Beter.TestingTools.Generator.UnitTests.Domain.TestScenarios
{
    public class TestScenarioMessageTests
    {
        [Fact]
        public void ToFeedMessages_ReturnsFeedMessageWrappers()
        {
            // Arrange
            var messageValue = JsonNode.Parse("[{\"id\": 1, \"name\": \"Message 1\"}, {\"id\": 2, \"name\": \"Message 2\"}]");
            var testMessage = new TestScenarioMessage { Value = messageValue };

            // Act
            var feedMessages = testMessage.ToFeedMessages();

            // Assert
            Assert.Equal(2, feedMessages.Count());
            Assert.All(feedMessages, feedMessage => Assert.IsType<FeedMessageWrapper>(feedMessage));
        }

        [Fact]
        public void IsMessageType_ReturnsTrue_WhenMessageTypeMatches()
        {
            // Arrange
            var testMessage = new TestScenarioMessage { MessageType = "Type1" };

            // Act & Assert
            Assert.True(testMessage.IsMessageType("Type1"));
        }

        [Fact]
        public void IsMessageType_ReturnsFalse_WhenMessageTypeDoesNotMatch()
        {
            // Arrange
            var testMessage = new TestScenarioMessage { MessageType = "Type1" };

            // Act & Assert
            Assert.False(testMessage.IsMessageType("Type2"));
        }

        [Fact]
        public void Modify_ModifiesValueCorrectly()
        {
            // Arrange
            var initialValue = new GlobalMessageModel
            {
                Id = "1",
                EventType = GlobalEventType.BetStateChange,
                EventValue = GlobalEventValue.GlobalBetStart
            };

            var testMessage = new TestScenarioMessage { Value = JsonNode.Parse(JsonSerializer.Serialize(initialValue)) };

            // Act
            var modifiedValue = testMessage.Modify<GlobalMessageModel>(value =>
            {
                value.EventValue = GlobalEventValue.GlobalBetStop;
            });

            // Assert
            Assert.Equal(GlobalEventValue.GlobalBetStop, modifiedValue.EventValue);
        }

        [Fact]
        public void GetValue_DeserializesValueCorrectly()
        {
            // Arrange
            var initialValue = new GlobalMessageModel
            {
                Id = "1",
                EventType = GlobalEventType.BetStateChange,
                EventValue = GlobalEventValue.GlobalBetStart
            };

            var testMessage = new TestScenarioMessage { Value = JsonNode.Parse(JsonSerializer.Serialize(initialValue)) };

            // Act
            var deserializedValue = testMessage.GetValue<GlobalMessageModel>();

            // Assert
            Assert.Equal(initialValue.Id, deserializedValue.Id);
            Assert.Equal(initialValue.EventValue, deserializedValue.EventValue);
            Assert.Equal(initialValue.EventType, deserializedValue.EventType);
        }
    }
}
