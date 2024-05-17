using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Application.Extensions;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations.Rules;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.UnitTests.Fixtures;
using Beter.Feed.TestingSandbox.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.Playbacks.Transformations.Rules
{
    public class SubscriptionsRemovedTransformationRuleTests
    {
        private static readonly Fixture _fixture = new();
        private readonly SubscriptionsRemovedTransformationRule _rule = new();

        public SubscriptionsRemovedTransformationRuleTests()
        {
            _fixture.Customizations.Add(new JsonNodeBuilder());
        }

        [Fact]
        public void IsApplicable_Returns_True_For_SubscriptionsRemoved_MessageType()
        {
            // Arrange
            var message = _fixture.Build<TestScenarioMessage>()
                .With(x => x.MessageType, MessageTypes.SubscriptionsRemoved)
                .Create();

            // Act
            var result = _rule.IsApplicable(message);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsApplicable_Returns_False_For_Non_SubscriptionsRemoved_MessageType()
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
        public void Transform_Calls_UpdateModel_Method_For_SubscriptionsRemoved()
        {
            // Arrange
            var model = new SubscriptionsRemovedModel
            {
                Ids = new[] { "originalMatchId1", "originalMatchId2", "originalMatchId3" }
            };

            var profile1 = SetupProfile("originalMatchId1", "newMatchId1");
            var profile2 = SetupProfile("originalMatchId2", "newMatchId2");
            var profile3 = SetupProfile("originalMatchId3", "newMatchId3");

            var context = _fixture.Build<MessagesTransformationContext>()
               .With(x => x.ReplyMode, ReplyMode.HistoricalTimeline)
               .With(x => x.Matches, new[] { profile1, profile2, profile3 }.ToDictionary(x => x.Id))
               .Create();

            var message = _fixture.Build<TestScenarioMessage>()
                .With(x => x.ScheduledAt, DateTime.UtcNow.ToUnixTimeMilliseconds())
                .With(x => x.Value, JsonNode.Parse(JsonSerializer.Serialize(model)))
                .Create();

            // Act
            _rule.Transform(context, message);

            // Assert
            var actual = message.GetValue<SubscriptionsRemovedModel>().Ids.ToList();

            Assert.Equal(3, actual.Count);
            Assert.Equal("newMatchId1", actual[0]);
            Assert.Equal("newMatchId2", actual[1]);
            Assert.Equal("newMatchId3", actual[2]);
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
