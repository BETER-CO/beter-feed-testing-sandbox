using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Generator.Application.Services;
using Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers.Offsets;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.TestScenarios.MessageHandlers.Offsets
{
    public class OffsetStorageTests
    {
        private static readonly Fixture Fixture = new();

        private readonly OffsetStorage _offsetStorage;

        public OffsetStorageTests()
        {
            _offsetStorage = new OffsetStorage(new SequenceNumberProvider());
        }

        [Fact]
        public void GetOffsetForUpdateMessage_AddsNewMatchId_WhenMatchIdDoesNotExist()
        {
            // Arrange
            var matchId = Fixture.Create<string>();
            var hubKind = HubKind.Incident;

            // Act
            var offset = _offsetStorage.GetOffsetForUpdateMessage(matchId, hubKind);

            // Assert
            Assert.Equal(1, offset);
        }

        [Fact]
        public void GetOffsetForUpdateMessage_UpdatesExistingMatchId_WithNewOffset()
        {
            // Arrange
            var matchId = Fixture.Create<string>();
            var hubKind = HubKind.Incident;

            // Act
            var initialOffset = _offsetStorage.GetOffsetForUpdateMessage(matchId, hubKind);
            var newOffset = _offsetStorage.GetOffsetForUpdateMessage(matchId, hubKind);

            // Assert
            Assert.Equal(1, initialOffset);
            Assert.Equal(2, newOffset);
        }

        [Fact]
        public void GetOffsetForNonUpdateMessage_ReturnsExistingOffsets_WhenMatchIdExists()
        {
            // Arrange
            var matchId = Fixture.Create<string>();
            var hubKind = HubKind.Incident;
            _offsetStorage.GetOffsetForUpdateMessage(matchId, hubKind);
            _offsetStorage.GetOffsetForUpdateMessage(matchId, hubKind);
            _offsetStorage.GetOffsetForUpdateMessage(matchId, hubKind);

            // Act
            var result = _offsetStorage.GetOffsetForNonUpdateMessage(matchId, hubKind);

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public void GetOffsetForNonUpdateMessage_AddsNewMatchId_WhenMatchIdDoesNotExist()
        {
            // Arrange
            var matchId = Fixture.Create<string>();
            var hubKind = HubKind.Incident;

            // Act
            var result = _offsetStorage.GetOffsetForNonUpdateMessage(matchId, hubKind);

            // Assert
            Assert.Equal(1, result);
        }
    }
}
