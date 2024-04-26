using Moq;
using AutoFixture;
using Beter.TestingTools.Generator.UnitTests.Fixtures;
using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Rules;
using Beter.TestingTools.Generator.Application.Contracts.Playbacks;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations;
using Beter.TestingTools.Generator.Application.Extensions;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.Playbacks.Transformations
{
    public class TransformationManagerTests
    {
        private static readonly Fixture _fixture = new();

        public TransformationManagerTests()
        {
            _fixture.Customizations.Add(new JsonNodeBuilder());
        }

        [Fact]
        public void Constructor_Throws_Exception_When_TransformationRules_Are_Null()
        {
            // Arrange
            IEnumerable<ITransformationRule> transformationRules = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => new TransformationManager(transformationRules));
        }

        [Fact]
        public void ApplyTransformation_Updates_ScheduledAt_And_Calls_Applicable_TransformationRule()
        {
            // Arrange
            var message1 = _fixture.Build<TestScenarioMessage>()
                .With(x => x.ScheduledAt, new DateTime(2024, 1, 1, 14, 0, 0).ToUnixTimeMilliseconds())
                .With(x => x.MessageType, MessageTypes.Scoreboard)
                .Create();
            var message2 = _fixture.Build<TestScenarioMessage>()
                .With(x => x.ScheduledAt, new DateTime(2024, 1, 1, 15, 30, 0).ToUnixTimeMilliseconds())
                .With(x => x.MessageType, MessageTypes.Incident)
                .Create();

            var testCaseStart = new DateTime(2024, 1, 1, 20, 0, 0);
            var context = _fixture.Build<MessagesTransformationContext>()
               .With(x => x.TestCaseStart, testCaseStart)
               .With(x => x.ReplyMode, ReplyMode.HistoricalTimeline)
               .With(x => x.OldFirstMessageScheduledAt, new DateTime(2024, 1, 1, 14, 0, 0).ToUnixTimeMilliseconds())
               .With(x => x.NewFirstMessageScheduledAt, testCaseStart.ToUnixTimeMilliseconds())
               .Create();

            var transformationRule = new Mock<ITransformationRule>();
            transformationRule.Setup(rule => rule.IsApplicable(
                It.Is<TestScenarioMessage>(x => x.MessageType == MessageTypes.Scoreboard)))
                .Returns(true);

            var manager = new TransformationManager(new[] { transformationRule.Object });

            // Act
            var result = manager.ApplyTransformation(context, new[] { message1, message2 }).ToList();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(new DateTime(2024, 1, 1, 20, 0, 0).ToUnixTimeMilliseconds(), result[0].ScheduledAt);
            Assert.Equal(new DateTime(2024, 1, 1, 21, 30, 0).ToUnixTimeMilliseconds(), result[1].ScheduledAt);

            transformationRule.Verify(
                rule => rule.Transform(context, message1), Times.Once);
        }
    }
}
