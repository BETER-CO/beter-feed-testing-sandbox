using AutoFixture;
using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Generator.UnitTests.Fixtures;
using Beter.TestingTools.Models.Incidents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Text.Json;
using Beter.TestingTools.Models.GlobalEvents;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Rules;
using Beter.TestingTools.Generator.Application.Contracts.Playbacks;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations;
using Beter.TestingTools.Generator.Application.Extensions;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.Playbacks.Transformations.Rules
{
    public class SystemEventTransformationRuleTests
    {
        private static readonly Fixture _fixture = new();
        private readonly SystemEventTransformationRule _rule = new();

        public SystemEventTransformationRuleTests()
        {
            _fixture.Customizations.Add(new JsonNodeBuilder());
        }

        [Fact]
        public void IsApplicable_Returns_True_For_SystemEvent_MessageType()
        {
            // Arrange
            var message = _fixture.Build<TestScenarioMessage>()
                .With(x => x.MessageType, MessageTypes.SystemEvent)
                .Create();

            // Act
            var result = _rule.IsApplicable(message);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsApplicable_Returns_False_For_Non_SystemEvent_MessageType()
        {
            // Arrange
            var message = _fixture.Build<TestScenarioMessage>()
                .With(x => x.MessageType, MessageTypes.Scoreboard)
                .Create();

            // Act
            var result = _rule.IsApplicable(message);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Transform_Calls_UpdateModel_Method_For_Each_SystemEvent()
        {
            // Arrange
            var (model1, profile1) = SetupModelAndProfile("originalMatchId1", "newMatchId1");
            var (model2, profile2) = SetupModelAndProfile("originalMatchId2", "newMatchId2");
            var (model3, profile3) = SetupModelAndProfile("originalMatchId3", "newMatchId3");

            var testStartDate = DateTime.SpecifyKind(new DateTime(2024, 4, 20), DateTimeKind.Utc);
            var context = SetupMessagesTransformationContext(testStartDate, profile1, profile2, profile3);

            var message = _fixture.Build<TestScenarioMessage>()
                .With(x => x.ScheduledAt, DateTime.UtcNow.ToUnixTimeMilliseconds())
                .With(x => x.Value, JsonNode.Parse(JsonSerializer.Serialize(new[] { model1, model2, model3 })))
                .Create();

            // Act
            _rule.Transform(context, message);

            // Assert
            var models = message.GetValue<List<GlobalMessageModel>>();

            Assert.Equal("newMatchId1", models[0].Id);
            Assert.Equal("newMatchId2", models[1].Id);
            Assert.Equal("newMatchId3", models[2].Id);
        }

        private MessagesTransformationContext SetupMessagesTransformationContext(DateTime testCaseStart, params MessagesTransformationContext.MatchIdProfile[] matches)
        {
            return _fixture.Build<MessagesTransformationContext>()
                .With(x => x.TestCaseStart, testCaseStart)
                .With(x => x.ReplyMode, ReplyMode.HistoricalTimeline)
                .With(x => x.Matches, matches.ToDictionary(x => x.Id))
                .Create();
        }

        private static (GlobalMessageModel model, MessagesTransformationContext.MatchIdProfile profile) SetupModelAndProfile(string originalMatchId, string newMatchId)
        {
            var model = SetupModel(originalMatchId);
            var profile = SetupProfile(originalMatchId, newMatchId);
            return (model, profile);
        }

        private static GlobalMessageModel SetupModel(string matchId)
        {
            return _fixture.Build<GlobalMessageModel>()
              .With(x => x.Id, matchId)
              .Create();
        }

        private static MessagesTransformationContext.MatchIdProfile SetupProfile(string oldMatchId, string newMatchId)
        {
            var profile = _fixture.Build<MessagesTransformationContext.MatchIdProfile>()
                .With(x => x.Id, oldMatchId)
                .With(x => x.NewId, newMatchId)
                .With(x => x.WasFirstTimeTableMessage, false)
                .With(x => x.IsFirstTimeTableMessageDelayProcessed, false)
                .Create();

            return profile;
        }
    }
}
