using AutoFixture;
using Beter.Feed.TestingSandbox.Generator.Application.Mappers;
using Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;
using Beter.Feed.TestingSandbox.Generator.UnitTests.Fixtures;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Mappers
{
    public class PlaybackMapperTests
    {
        private readonly Fixture _fixture;

        public PlaybackMapperTests()
        {
            _fixture = new();
            _fixture.Customizations.Add(new JsonNodeBuilder());
        }

        [Fact]
        public void MapToDto_ReturnsCorrectPlaybackDto()
        {
            // Arrange
            var source = _fixture.Create<Playback>();

            // Act
            var result = PlaybackMapper.MapToDto(source);

            // Assert
            Assert.Equal(source.Id, result.PlaybackId);
            Assert.Equal(source.CaseId, result.CaseId);
            Assert.Equal(source.Description, result.Description);
            Assert.Equal(source.Version.ToString(), result.Version);
            Assert.Equal(source.StartedAt, result.StartedAt);
            Assert.Equal(source.ActiveMessagesCount, result.ActiveMessagesCount);
            Assert.Equal(source.LastMessageSentAt, result.LastMessageSentAt);
        }

        [Fact]
        public void MapToStopPlaybackItemResponse_ReturnsCorrectStopPlaybackItemResponse()
        {
            // Arrange
            var source = _fixture.Create<Playback>();

            // Act
            var result = PlaybackMapper.MapToStopPlaybackItemResponse(source);

            // Assert
            Assert.Equal(source.Id, result.PlaybackId);
            Assert.Equal(source.CaseId, result.TestCaseId);
            Assert.Equal(source.Description, result.Description);
            Assert.Equal(source.Version.ToString(), result.Version);
        }
    }
}
