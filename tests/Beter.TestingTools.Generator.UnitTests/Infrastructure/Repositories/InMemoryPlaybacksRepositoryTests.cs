using AutoFixture;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.Application.Common;
using Beter.TestingTools.Generator.Application.Contracts;
using Beter.TestingTools.Generator.Domain.Playbacks;
using Beter.TestingTools.Generator.Infrastructure.Repositories;
using Beter.TestingTools.Generator.UnitTests.Fixtures;
using Moq;

namespace Beter.TestingTools.Generator.UnitTests.Infrastructure.Repositories
{
    public class InMemoryPlaybacksRepositoryTests
    {
        private static readonly Fixture Fixture = new();

        private readonly Mock<ISystemClock> _systemClock;
        private readonly InMemoryPlaybacksRepository _repository;

        public InMemoryPlaybacksRepositoryTests()
        {
            Fixture.Customizations.Add(new JsonNodeBuilder());

            _systemClock = new Mock<ISystemClock>();
            _repository = new InMemoryPlaybacksRepository(_systemClock.Object);
        }

        [Fact]
        public void Add_AddsPlaybackCorrectly()
        {
            // Arrange
            var playback1 = Fixture.Create<Playback>();
            var playback2 = Fixture.Create<Playback>();

            // Act
            _repository.Add(playback1);
            _repository.Add(playback2);

            // Assert
            Assert.Equal(playback1, _repository.Get(playback1.Id));
            Assert.Equal(playback2, _repository.Get(playback2.Id));
        }

        [Fact]
        public void Add_ThrowsArgumentNullException_WhenPlaybackIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => _repository.Add(null));
        }

        [Fact]
        public void Add_DuplicateEntityException_WhenPlaybackWithSameIdAlreadyExists()
        {
            // Arrange
            var playback = Fixture.Create<Playback>();
            _repository.Add(playback);

            // Act & Assert
            Assert.Throws<DuplicateEntityException>(
                () => _repository.Add(playback));
        }

        [Fact]
        public void Remove_RemovesPlaybackCorrectly()
        {
            // Arrange
            var playback = Fixture.Create<Playback>();
            _repository.Add(playback);

            // Act
            var removedPlayback = _repository.Remove(playback.Id);

            // Assert
            Assert.Null(_repository.Get(playback.Id));
            Assert.Equal(playback, removedPlayback);
        }

        [Fact]
        public void Remove_ThrowsArgumentNullException_WhenPlaybackIdIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => _repository.Remove(null));
        }

        [Fact]
        public void Remove_ThrowsRequiredEntityNotFoundException_WhenPlaybackIdDoesNotExist()
        {
            // Act & Assert
            Assert.Throws<RequiredEntityNotFoundException>(
                () => _repository.Remove("nonexistentId"));
        }

        [Fact]
        public void RemoveAll_RemovesAllPlaybacks()
        {
            // Arrange
            var playback1 = Fixture.Create<Playback>();
            var playback2 = Fixture.Create<Playback>();

            _repository.Add(playback1);
            _repository.Add(playback2);

            // Act
            _repository.RemoveAll();

            // Assert
            Assert.Null(_repository.Get(playback1.Id));
            Assert.Null(_repository.Get(playback2.Id));
        }

        [Fact]
        public void Get_ReturnsPlaybackCorrectly()
        {
            // Arrange
            var playback = Fixture.Create<Playback>();
            _repository.Add(playback);

            // Act
            var retrievedPlayback = _repository.Get(playback.Id);

            // Assert
            Assert.Equal(playback, retrievedPlayback);
        }

        [Fact]
        public void GetActive_ReturnsActivePlaybacks()
        {
            // Arrange
            _repository.Add(new Playback { Id = "playback1", Messages = Fixture.Create<Dictionary<string, PlaybackItem>>()});
            _repository.Add(new Playback { Id = "playback2" });

            // Act
            var activePlaybacks = _repository.GetActive();

            // Assert
            Assert.Collection(activePlaybacks,
                playback => Assert.Equal("playback1", playback.Id));
        }

        [Fact]
        public void RemoveMessageFromPlayback_RemovesMessageFromPlayback()
        {
            // Arrange
            var playbackItem = Fixture.Build<PlaybackItem>()
                .With(x => x.Message, new TestScenarioMessage())
                .Create();
            var playback = Fixture.Build<Playback>()
                .With(x => x.Id, playbackItem.PlaybackId)
                .With(x => x.Messages, new Dictionary<string, PlaybackItem> { { playbackItem.InternalId, playbackItem } })
                .Create();

            _repository.Add(playback);

            // Act
            _repository.RemoveMessageFromPlayback(playbackItem.PlaybackId, playbackItem);

            // Assert
            Assert.Equal(0, _repository.Get(playbackItem.PlaybackId).ActiveMessagesCount);
            Assert.Empty(_repository.Get(playbackItem.PlaybackId).Messages);
        }

        [Fact]
        public void GetNearestRunTime_ReturnsNearestRunTime()
        {
            // Arrange
            var utcNow = DateTimeOffset.UtcNow;
            _systemClock.Setup(clock => clock.UtcNow).Returns(utcNow);

            var currentTime = utcNow.ToUnixTimeMilliseconds();
            var playback1 = Fixture.Build<Playback>()
                .With(x => x.Messages, new Dictionary<string, PlaybackItem>
                {
                    { "message1", new PlaybackItem { Message = new TestScenarioMessage { ScheduledAt = currentTime } } }
                })
                .Create();

            var playback2 = Fixture.Build<Playback>()
                .With(x => x.Messages, new Dictionary<string, PlaybackItem>
                {
                    { "message2", new PlaybackItem { Message = new TestScenarioMessage { ScheduledAt = currentTime + 1000 } } }
                })
                .Create();

            _repository.Add(playback1);
            _repository.Add(playback2);

            // Act
            var nearestRunTime = _repository.GetNearestRunTime();

            // Assert
            Assert.Equal(currentTime, nearestRunTime?.ToUnixTimeMilliseconds());
        }

        [Fact]
        public void GetMessagesToExecute_ReturnsMessagesToExecute()
        {
            // Arrange
            var utcNow = DateTimeOffset.UtcNow;
            _systemClock.Setup(clock => clock.UtcNow).Returns(utcNow);

            var currentTime = utcNow.ToUnixTimeMilliseconds();
            var expected = new PlaybackItem { Message = new TestScenarioMessage { ScheduledAt = currentTime } };
            var playback1 = Fixture.Build<Playback>()
                .With(x => x.Messages, new Dictionary<string, PlaybackItem>
                {
                    { "message1", expected }
                })
                .Create();

            var playback2 = Fixture.Build<Playback>()
                .With(x => x.Messages, new Dictionary<string, PlaybackItem>
                {
                    { "message2", new PlaybackItem { Message = new TestScenarioMessage { ScheduledAt = currentTime + 1000 } } }
                })
                .Create();

            _repository.Add(playback1);
            _repository.Add(playback2);

            // Act
            var messagesToExecute = _repository.GetMessagesToExecute().ToList();

            // Assert
            Assert.Single(messagesToExecute);
            Assert.Collection(messagesToExecute,
             playback => Assert.Equal(expected, messagesToExecute[0]));
        }
    }
}
