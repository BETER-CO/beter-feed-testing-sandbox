using Moq;
using AutoFixture;
using Beter.TestingTools.Generator.UnitTests.Fixtures;
using Beter.TestingTools.Generator.UnitTests.Common;
using Beter.TestingTools.Generator.Application.Contracts;
using Beter.TestingTools.Generator.Application.Contracts.Playbacks;
using Beter.TestingTools.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations;
using Beter.TestingTools.Generator.Application.Services.Playbacks;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.Playbacks
{
    public class PlaybackFactoryTests
    {
        private static readonly Fixture _fixture = new();

        private readonly Mock<ISystemClock> _systemClock;
        private readonly Mock<ITestScenariosRepository> _repository;
        private readonly Mock<ITransformationManager> _manager;
        private readonly Mock<IMessagesTransformationContextFactory> _contextFactory;

        private readonly PlaybackFactory _playbackFactory;

        public PlaybackFactoryTests()
        {
            _fixture.Customizations.Add(new JsonNodeBuilder());
            _systemClock = new Mock<ISystemClock>();
            _repository = new Mock<ITestScenariosRepository>();
            _manager = new Mock<ITransformationManager>();
            _contextFactory = new Mock<IMessagesTransformationContextFactory>();

            _playbackFactory = new PlaybackFactory(
                _systemClock.Object,
                _repository.Object,
                _manager.Object,
                _contextFactory.Object);
        }

        [Fact]
        public void Ctor_should_has_null_guards() => 
            AssertInjection.OfConstructor(typeof(PlaybackFactory)).HasNullGuard();

        [Fact]
        public void Create_ReturnsPlayback()
        {
            // Arrange
            var replyMode = ReplyMode.HistoricalTimeline;
            var timeOffsetInMinutes = _fixture.Create<int>();
            var timeOffsetBetweenMessagesInSecounds = _fixture.Create<int>();
            var timeOffsetAfterFirstTimetableMessageInSecounds = _fixture.Create<int>();
            var accelerationFactor = _fixture.Create<double>();
            var utcNow = DateTime.UtcNow;

            var testScenario = _fixture.Create<TestScenario>();
            _systemClock.Setup(c => c.UtcNow).Returns(utcNow);

            var context = _fixture.Create<MessagesTransformationContext>();
            _contextFactory.Setup(x => x.Create(testScenario.CaseId, replyMode, testScenario.Messages, TimeSpan.FromMinutes(timeOffsetInMinutes), TimeSpan.FromSeconds(timeOffsetAfterFirstTimetableMessageInSecounds),accelerationFactor))
                .Returns(context);

            _repository.Setup(r => r.Requre(testScenario.CaseId))
                .Returns(testScenario);

            // Act
            var playback = _playbackFactory.Create(
                testScenario.CaseId,
                replyMode,
                timeOffsetInMinutes,
                timeOffsetBetweenMessagesInSecounds,
                timeOffsetAfterFirstTimetableMessageInSecounds,
                accelerationFactor);

            // Assert
            Assert.NotNull(playback);
            Assert.Equal(testScenario.CaseId, playback.CaseId);
            Assert.Equal(testScenario.Description, playback.Description);
            Assert.Equal(testScenario.Version, playback.Version);
            Assert.Equal(testScenario.Messages.Count(), playback.ActiveMessagesCount);
            Assert.Equal(utcNow.AddMinutes(timeOffsetInMinutes), playback.StartedAt);
            Assert.Equal(0, playback.LastMessageSentAt);

            _manager.Verify(x => x.ApplyTransformation(context, testScenario.Messages), Times.Once);
        }
    }
}
