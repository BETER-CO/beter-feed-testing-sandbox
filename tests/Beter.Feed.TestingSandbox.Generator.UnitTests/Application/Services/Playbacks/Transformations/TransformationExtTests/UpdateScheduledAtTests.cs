using AutoFixture;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Application.Extensions;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.UnitTests.Fixtures;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.Playbacks.Transformations.TransformationExtTests
{
    public class UpdateScheduledAtTests
    {
        private readonly Fixture _fixture = new();

        public UpdateScheduledAtTests()
        {
            _fixture.Customizations.Add(new JsonNodeBuilder());
        }

        [Fact]
        public void UpdateScheduledAt_Updates_ScheduledAt_When_Profile_Indicates_FirstTimeTableMessage_And_Not_Processed()
        {
            // Arrange
            var model = _fixture.Build<TestModel>()
                .With(x => x.Id, _fixture.Create<string>())
                .Create();

            var timeOffset = TimeSpan.FromSeconds(10);
            var profile = _fixture.Build<MessagesTransformationContext.MatchIdProfile>()
                .With(x => x.Id, model.Id)
                .With(x => x.WasFirstTimeTableMessage, true)
                .With(x => x.IsFirstTimeTableMessageDelayProcessed, false)
                .Create();
            var context = _fixture.Build<MessagesTransformationContext>()
                .With(x => x.Matches, new Dictionary<string, MessagesTransformationContext.MatchIdProfile> { { model.Id, profile } })
                .With(x => x.TimeOffsetAfterFirstTimetableMessageInSecounds, timeOffset)
                .Create();

            var originalScheduledAt = new DateTime(2024, 04, 25).ToUnixTimeMilliseconds();
            var expectedScheduledAt = new DateTime(2024, 04, 25, 0, 0, 10).ToUnixTimeMilliseconds();
            var message = new TestScenarioMessage { ScheduledAt = originalScheduledAt };

            // Act
            TransformationsExt.UpdateScheduledAt(model, message, context);

            // Assert
            Assert.NotEqual(originalScheduledAt, message.ScheduledAt);
            Assert.Equal(expectedScheduledAt, message.ScheduledAt);
            Assert.True(profile.IsFirstTimeTableMessageDelayProcessed);
        }

        [Fact]
        public void UpdateScheduledAt_Does_Not_Update_ScheduledAt_When_Profile_Does_Not_Indicate_FirstTimeTableMessage()
        {
            // Arrange
            var model = _fixture.Build<TestModel>()
                .With(x => x.Id, _fixture.Create<string>())
                .Create();

            var timeOffset = TimeSpan.FromSeconds(10);
            var profile = _fixture.Build<MessagesTransformationContext.MatchIdProfile>()
                .With(x => x.Id, model.Id)
                .With(x => x.WasFirstTimeTableMessage, false)
                .With(x => x.IsFirstTimeTableMessageDelayProcessed, false)
                .Create();
            var context = _fixture.Build<MessagesTransformationContext>()
                .With(x => x.Matches, new Dictionary<string, MessagesTransformationContext.MatchIdProfile> { { model.Id, profile } })
                .With(x => x.TimeOffsetAfterFirstTimetableMessageInSecounds, timeOffset)
                .Create();

            var originalScheduledAt = new DateTime(2024, 04, 25).ToUnixTimeMilliseconds();
            var message = new TestScenarioMessage { ScheduledAt = originalScheduledAt };

            // Act
            TransformationsExt.UpdateScheduledAt(model, message, context);

            // Assert
            Assert.Equal(originalScheduledAt, message.ScheduledAt);
            Assert.False(profile.IsFirstTimeTableMessageDelayProcessed);
        }

        [Fact]
        public void UpdateScheduledAt_Does_Not_Update_ScheduledAt_When_Profile_Indicates_FirstTimeTableMessage_But_Already_Processed()
        {
            // Arrange
            var model = _fixture.Build<TestModel>()
              .With(x => x.Id, _fixture.Create<string>())
              .Create();

            var timeOffset = TimeSpan.FromSeconds(10);
            var profile = _fixture.Build<MessagesTransformationContext.MatchIdProfile>()
                .With(x => x.Id, model.Id)
                .With(x => x.WasFirstTimeTableMessage, true)
                .With(x => x.IsFirstTimeTableMessageDelayProcessed, true)
                .Create();
            var context = _fixture.Build<MessagesTransformationContext>()
                .With(x => x.Matches, new Dictionary<string, MessagesTransformationContext.MatchIdProfile> { { model.Id, profile } })
                .With(x => x.TimeOffsetAfterFirstTimetableMessageInSecounds, timeOffset)
                .Create();

            var originalScheduledAt = new DateTime(2024, 04, 25).ToUnixTimeMilliseconds();
            var message = new TestScenarioMessage { ScheduledAt = originalScheduledAt };

            // Act
            TransformationsExt.UpdateScheduledAt(model, message, context);

            // Assert
            Assert.Equal(originalScheduledAt, message.ScheduledAt);
            Assert.True(profile.IsFirstTimeTableMessageDelayProcessed);
        }

        [Fact]
        public void UpdateScheduledAt_Updates_ScheduledAt_When_ReplyMode_Is_HistoricalTimeline()
        {
            // Arrange
            var oldFirstMessageScheduledAtTimestamp = DateTime.SpecifyKind(new DateTime(2024, 4, 1), DateTimeKind.Utc).ToUnixTimeMilliseconds();
            var newFirstMessageScheduledAtTimestamp = DateTime.SpecifyKind(new DateTime(2024, 4, 20), DateTimeKind.Utc).ToUnixTimeMilliseconds();
            var originalScheduledAt = DateTime.SpecifyKind(new DateTime(2024, 4, 10), DateTimeKind.Utc).ToUnixTimeMilliseconds();
            var message = _fixture.Build<TestScenarioMessage>()
                .With(x => x.ScheduledAt, originalScheduledAt)
                .Create();

            var context = new MessagesTransformationContext
            {
                ReplyMode = ReplyMode.HistoricalTimeline,
                OldFirstMessageScheduledAt = oldFirstMessageScheduledAtTimestamp,
                NewFirstMessageScheduledAt = newFirstMessageScheduledAtTimestamp
            };

            var expectedScheduledAt = DateTime.SpecifyKind(new DateTime(2024, 4, 29), DateTimeKind.Utc).ToUnixTimeMilliseconds();

            // Act
            TransformationsExt.UpdateScheduledAt(message, context);

            // Assert
            Assert.Equal(expectedScheduledAt, message.ScheduledAt);
        }

        [Fact]
        public void UpdateScheduledAt_Throws_NotImplementedException_When_ReplyMode_Is_Not_HistoricalTimeline()
        {
            // Arrange
            var message = _fixture.Create<TestScenarioMessage>();
            var context = _fixture.Build<MessagesTransformationContext>()
                .With(x => x.ReplyMode, ReplyMode.FixedDelay)
                .Create();

            // Act & Assert
            Assert.Throws<NotImplementedException>(
                () => TransformationsExt.UpdateScheduledAt(message, context));
        }
    }
}
