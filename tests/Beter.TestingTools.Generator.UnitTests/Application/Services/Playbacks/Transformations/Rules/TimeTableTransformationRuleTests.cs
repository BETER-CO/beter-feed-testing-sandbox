using AutoFixture;
using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Generator.Application.Contracts.Playbacks;
using Beter.TestingTools.Generator.Application.Extensions;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Rules;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.UnitTests.Fixtures;
using Beter.TestingTools.Models.TimeTableItems;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.Playbacks.Transformations.Rules
{
    public class TimeTableTransformationRuleTests
    {
        private static readonly Fixture _fixture = new();
        private readonly TimeTableTransformationRule _rule = new();

        public TimeTableTransformationRuleTests()
        {
            _fixture.Customizations.Add(new JsonNodeBuilder());
        }

        [Fact]
        public void IsApplicable_Returns_True_For_TimeTable_MessageType()
        {
            // Arrange
            var message = _fixture.Build<TestScenarioMessage>()
                .With(x => x.MessageType, MessageTypes.Timetable)
                .Create();

            // Act
            var result = _rule.IsApplicable(message);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsApplicable_Returns_False_For_Non_imeTable_MessageType()
        {
            // Arrange
            var message = _fixture.Build<TestScenarioMessage>()
                .With(x => x.MessageType, MessageTypes.Incident)
                .Create();

            // Act
            var result = _rule.IsApplicable(message);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Transform_Calls_UpdateModel_StartDate_For_Each_TimeTableItemModel()
        {
            // Arrange
            var oldMatchId = "oldMatchId";
            var newMatchId = "newMatchId";
            var model1 = SetupModel(oldMatchId, new DateTime(2024, 1, 1, 13, 0, 0), new DateTime(2024, 1, 1, 14, 0, 0));
            var model2 = SetupModel(oldMatchId, new DateTime(2024, 1, 1, 16, 30, 0), new DateTime(2024, 1, 1, 17, 0, 0));

            var profile = _fixture.Build<MessagesTransformationContext.MatchIdProfile>()
                .With(x => x.Id, oldMatchId)
                .With(x => x.NewId, newMatchId)
                .With(x => x.OldStartDate, model1.StartDate)
                .With(x => x.NewStartDate, DateTime.SpecifyKind(new DateTime(2024, 1, 1, 19, 0, 0), DateTimeKind.Utc))
                .With(x => x.OldFirstTimestampByEachMessageType, new Dictionary<string, DateTime>
                {
                    { MessageTypes.Timetable, new DateTime(2024, 1, 1, 13, 0, 0) },
                })
                .With(x => x.WasFirstTimeTableMessage, false)
                .With(x => x.IsFirstTimeTableMessageDelayProcessed, false)
              .Create();

            var testStartDate = new DateTime(2024, 1, 1, 18, 0, 0);
            var context = SetupMessagesTransformationContext(testStartDate, profile);

            var message1 = _fixture.Build<TestScenarioMessage>()
                .With(x => x.ScheduledAt, DateTime.UtcNow.ToUnixTimeMilliseconds())
                .With(x => x.Value, JsonNode.Parse(JsonSerializer.Serialize(new[] { model1 })))
                .Create();

            var message2 = _fixture.Build<TestScenarioMessage>()
                .With(x => x.ScheduledAt, DateTime.UtcNow.ToUnixTimeMilliseconds())
                .With(x => x.Value, JsonNode.Parse(JsonSerializer.Serialize(new[] { model2 })))
                .Create();

            // Act
            _rule.Transform(context, message1);
            _rule.Transform(context, message2);

            // Assert
            var actual = message1.GetValue<List<TimeTableItemModel>>().First();

            Assert.Equal(newMatchId, actual.Id);
            Assert.Equal(new DateTime(2024, 1, 1, 18, 0, 0).ToUnixTimeMilliseconds(), actual.Timestamp);
            Assert.Equal(new DateTime(2024, 1, 1, 19, 0, 00), actual.StartDate.Value);

            actual = message2.GetValue<List<TimeTableItemModel>>().First();

            Assert.Equal(newMatchId, actual.Id);
            Assert.Equal(new DateTime(2024, 1, 1, 21, 30, 0).ToUnixTimeMilliseconds(), actual.Timestamp);
            Assert.Equal(new DateTime(2024, 1, 1, 22, 0, 00), actual.StartDate.Value);

            Assert.Equal(new DateTime(2024, 1, 1, 17, 0, 0), context.GetMatchProfile(oldMatchId).OldStartDate);
        }

        private MessagesTransformationContext SetupMessagesTransformationContext(DateTime testCaseStart, params MessagesTransformationContext.MatchIdProfile[] matches)
        {
            testCaseStart = DateTime.SpecifyKind(testCaseStart, DateTimeKind.Utc);

            return _fixture.Build<MessagesTransformationContext>()
                .With(x => x.TestCaseStart, testCaseStart)
                .With(x => x.AccelerationFactor, 1)
                .With(x => x.ReplyMode, ReplyMode.HistoricalTimeline)
                .With(x => x.Matches, matches.ToDictionary(x => x.Id))
                .With(x => x.NewFirstMessageScheduledAt, testCaseStart.ToUnixTimeMilliseconds())
                .With(x => x.OldFirstMessageScheduledAt, DateTime.SpecifyKind(new DateTime(2024, 1, 1, 13, 0, 0), DateTimeKind.Utc).ToUnixTimeMilliseconds())
                .Create();
        }

        private static TimeTableItemModel SetupModel(string matchId, DateTime timestamp, DateTime oldStartDate)
        {
            return _fixture.Build<TimeTableItemModel>()
              .With(x => x.Id, matchId)
              .With(x => x.Timestamp, timestamp.ToUnixTimeMilliseconds())
              .With(x => x.StartDate, oldStartDate)
              .Create();
        }
    }
}
