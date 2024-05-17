using Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.Playbacks.Transformations
{
    public class MatchIdGeneratorTests
    {
        private static readonly MatchIdGenerator MatchIdGenerator = new();

        [Fact]
        public void Generate_ReturnsValidMatchId()
        {
            // Arrange
            var testCaseId = 123;
            var runCount = 45;

            // Act
            var matchId = MatchIdGenerator.Generate(testCaseId, runCount);

            // Assert
            Assert.NotNull(matchId);
            Assert.NotEmpty(matchId);
            Assert.Matches("00000123-[A-Fa-f0-9]+-00000045", matchId);
        }

        [Fact]
        public void Generate_GeneratesUniqueMatchIds()
        {
            // Arrange
            var testCaseId = 10;
            var runCount = 120;
            var matchIdCount = 100;

            // Act
            var matchIds = Enumerable.Range(1, matchIdCount)
                .Select(_ => MatchIdGenerator.Generate(testCaseId, runCount))
                .ToList();

            // Assert
            Assert.Equal(matchIdCount, matchIds.Distinct().Count());
        }
    }
}
