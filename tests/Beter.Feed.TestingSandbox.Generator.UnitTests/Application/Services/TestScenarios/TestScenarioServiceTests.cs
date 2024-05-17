using AutoFixture;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Application.Mappers;
using Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.Generator.Contracts.Responses;
using Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.UnitTests.Common;
using Beter.Feed.TestingSandbox.Generator.UnitTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.TestScenarios
{
    public class TestScenarioServiceTests
    {
        private readonly Fixture _fixture;

        private readonly Mock<IPlaybackFactory> _playbackFactory;
        private readonly Mock<IPlaybackRepository> _playbackRepository;
        private readonly Mock<ITestScenariosRepository> _testScenariosRepository;

        private readonly TestScenarioService _service;

        public TestScenarioServiceTests()
        {
            _fixture = new();
            _fixture.Customizations.Add(new JsonNodeBuilder());
            _playbackFactory = new Mock<IPlaybackFactory>();
            _playbackRepository = new Mock<IPlaybackRepository>();
            _testScenariosRepository = new Mock<ITestScenariosRepository>();

            _service = new TestScenarioService(
                _playbackFactory.Object,
                _playbackRepository.Object,
                _testScenariosRepository.Object,
                new NullLogger<TestScenarioService>());
        }

        [Fact]
        public void Ctor_should_has_null_guards()
        {
            AssertInjection.OfConstructor(typeof(TestScenarioService)).HasNullGuard();
        }

        [Fact]
        public void GetActivePlaybacks_ReturnsPlaybacks()
        {
            // Arrange
            var playbacks = _fixture.CreateMany<Playback>();
            _playbackRepository.Setup(repo => repo.GetActive())
                .Returns(playbacks);

            var expected = playbacks.Select(PlaybackMapper.MapToDto);

            // Act
            var actual = _service.GetActivePlaybacks();

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetAll_ReturnsTestScenarios()
        {
            // Arrange
            var testScenarios = _fixture.Create<Dictionary<int, TestScenario>>();
            _testScenariosRepository.Setup(repo => repo.GetAll())
                .Returns(testScenarios);

            var expected = testScenarios.ToDictionary(
                kv => kv.Key,
                kv => TestScenarioMapper.MapToDto(kv.Value));

            // Act
            var actual = _service.GetAll();

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Start_AddsPlayback()
        {
            // Arrange
            var request = _fixture.Create<StartPlaybackRequest>();

            var playback = _fixture.Create<Playback>();
            _playbackFactory.Setup(x => x.Create(request.CaseId, ReplyMode.HistoricalTimeline, request.TimeOffsetInMinutes, null, request.TimeOffsetAfterFirstTimetableMessageInSecounds, 1))
                .Returns(playback);

            var expected = PlaybackMapper.MapToDto(playback);

            // Act
            var actual = _service.Start(request);

            // Assert
            actual.Should().BeEquivalentTo(expected);

            _playbackRepository.Verify(repo => repo.Add(playback), Times.Once);
        }

        [Fact]
        public void Stop_WithStopSingle_RemovesPlayback()
        {
            // Arrange
            var request = _fixture.Build<StopPlaybackRequest>()
                .With(x => x.Command, StopPlaybackCommand.StopSingle)
                .Create();

            var playback = _fixture.Create<Playback>();
            _playbackRepository.Setup(x => x.Remove(request.PlaybackId))
                .Returns(playback);

            var expected = new StopPlaybackResponse
            {
                Command = request.Command,
                Items = new[] { PlaybackMapper.MapToStopPlaybackItemResponse(playback) }
            };

            // Act
            var actual = _service.Stop(request);

            // Assert
            actual.Should().BeEquivalentTo(expected);

            _playbackRepository.Verify(x => x.Remove(request.PlaybackId), Times.Once);
        }

        [Fact]
        public void Stop_WithStopAll_RemovesPlayback()
        {
            // Arrange
            var request = _fixture.Build<StopPlaybackRequest>()
                .With(x => x.Command, StopPlaybackCommand.StopAll)
                .Create();

            var playbacks = _fixture.CreateMany<Playback>();
            _playbackRepository.Setup(x => x.RemoveAll())
                .Returns(playbacks);

            var expected = new StopPlaybackResponse
            {
                Command = request.Command,
                Items = playbacks.Select(PlaybackMapper.MapToStopPlaybackItemResponse)
            };

            // Act
            var actual = _service.Stop(request);

            // Assert
            actual.Should().BeEquivalentTo(expected);

            _playbackRepository.Verify(x => x.RemoveAll(), Times.Once);
        }
    }
}
