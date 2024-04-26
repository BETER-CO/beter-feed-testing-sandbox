using AutoFixture;
using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Generator.UnitTests.Fixtures;
using Beter.TestingTools.Models.Scoreboards;
using System.Text.Json.Nodes;
using Beter.TestingTools.Models.TradingInfos;
using System.Text.Json;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Rules;
using Beter.TestingTools.Generator.Application.Contracts.Playbacks;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations;
using Beter.TestingTools.Generator.Application.Extensions;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.Playbacks.Transformations.Rules
{
    public class TradingTransformationRuleTests
    {
        private static readonly Fixture _fixture = new();
        private readonly TradingTransformationRule _rule = new();

        public TradingTransformationRuleTests()
        {
            _fixture.Customizations.Add(new JsonNodeBuilder());
        }

        [Fact]
        public void IsApplicable_Returns_True_For_Trading_MessageType()
        {
            // Arrange
            var message = _fixture.Build<TestScenarioMessage>()
                .With(x => x.MessageType, MessageTypes.Trading)
                .Create();

            // Act
            var result = _rule.IsApplicable(message);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsApplicable_Returns_False_For_Non_Trading_MessageType()
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
        public void Transform_Calls_UpdateModel_Method_For_Each_TradingInfoModel()
        {
            // Arrange
            var (model1, profile1) = SetupModelAndProfile("originalMatchId1", new DateTime(2024, 4, 10), "newMatchId1", new DateTime(2024, 4, 1));
            var (model2, profile2) = SetupModelAndProfile("originalMatchId2", new DateTime(2024, 4, 9), "newMatchId2", new DateTime(2024, 4, 5));
            var (model3, profile3) = SetupModelAndProfile("originalMatchId3", new DateTime(2024, 4, 5), "newMatchId3", new DateTime(2024, 4, 2));

            var testStartDate = DateTime.SpecifyKind(new DateTime(2024, 4, 20), DateTimeKind.Utc);
            var context = SetupMessagesTransformationContext(testStartDate, profile1, profile2, profile3);

            var message = _fixture.Build<TestScenarioMessage>()
                .With(x => x.ScheduledAt, DateTime.UtcNow.ToUnixTimeMilliseconds())
                .With(x => x.Value, JsonNode.Parse(JsonSerializer.Serialize(new[] { model1, model2, model3 })))
                .Create();

            // Act
            _rule.Transform(context, message);

            // Assert
            var models = message.GetValue<List<TradingInfoModel>>();

            Assert.Equal("newMatchId1", models[0].Id);
            Assert.Equal(new DateTime(2024, 4, 29).ToUnixTimeMilliseconds(), models[0].Timestamp);

            Assert.Equal("newMatchId2", models[1].Id);
            Assert.Equal(new DateTime(2024, 4, 24).ToUnixTimeMilliseconds(), models[1].Timestamp);

            Assert.Equal("newMatchId3", models[2].Id);
            Assert.Equal(new DateTime(2024, 4, 23).ToUnixTimeMilliseconds(), models[2].Timestamp);
        }

        private MessagesTransformationContext SetupMessagesTransformationContext(DateTime testCaseStart, params MessagesTransformationContext.MatchIdProfile[] matches)
        {
            return _fixture.Build<MessagesTransformationContext>()
                .With(x => x.TestCaseStart, testCaseStart)
                .With(x => x.ReplyMode, ReplyMode.HistoricalTimeline)
                .With(x => x.Matches, matches.ToDictionary(x => x.Id))
                .Create();
        }

        private static (TradingInfoModel model, MessagesTransformationContext.MatchIdProfile profile) SetupModelAndProfile(string originalMatchId, DateTime originalDate, string newMatchId, DateTime oldFirstMessageTimestamp)
        {
            var model = SetupModel(originalMatchId, originalDate);
            var profile = SetupProfile(originalMatchId, newMatchId, oldFirstMessageTimestamp);
            return (model, profile);
        }

        private static TradingInfoModel SetupModel(string matchId, DateTime date)
        {
            return _fixture.Build<TradingInfoModel>()
              .With(x => x.Id, matchId)
              .With(x => x.Timestamp, date.ToUnixTimeMilliseconds())
              .Create();
        }

        private static MessagesTransformationContext.MatchIdProfile SetupProfile(string oldMatchId, string newMatchId, DateTime oldFirstMessageTimestamp)
        {
            var profile = _fixture.Build<MessagesTransformationContext.MatchIdProfile>()
                .With(x => x.Id, oldMatchId)
                .With(x => x.NewId, newMatchId)
                .With(x => x.OldFirstTimestampByEachMessageType, new Dictionary<string, DateTime>
                {
                    { MessageTypes.Scoreboard,DateTime.UtcNow  },
                    { MessageTypes.Trading, oldFirstMessageTimestamp }
                })
                .With(x => x.WasFirstTimeTableMessage, false)
                .With(x => x.IsFirstTimeTableMessageDelayProcessed, false)
                .Create();

            return profile;
        }
    }
}
