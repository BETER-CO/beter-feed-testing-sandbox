using Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.Playbacks.Transformations
{
    public class MessagesTransformationContextTests
    {
        [Fact]
        public void GetMatchProfile_Returns_Profile_When_MatchId_Exists_In_Matches_Dictionary()
        {
            // Arrange
            var matchId = "123";
            var profile = new MessagesTransformationContext.MatchIdProfile { Id = matchId };
            var context = new MessagesTransformationContext { Matches = new Dictionary<string, MessagesTransformationContext.MatchIdProfile> { { matchId, profile } } };

            // Act
            var retrievedProfile = context.GetMatchProfile(matchId);

            // Assert
            Assert.Equal(profile, retrievedProfile);
        }

        [Fact]
        public void GetMatchProfile_Returns_Profile_When_MatchId_Exists_As_NewId_In_Matches_Dictionary()
        {
            // Arrange
            var matchId = "123";
            var profile = new MessagesTransformationContext.MatchIdProfile { NewId = matchId };
            var context = new MessagesTransformationContext { Matches = new Dictionary<string, MessagesTransformationContext.MatchIdProfile> { { "otherId", profile } } };

            // Act
            var retrievedProfile = context.GetMatchProfile(matchId);

            // Assert
            Assert.Equal(profile, retrievedProfile);
        }

        [Fact]
        public void GetMatchProfile_Throws_Exception_When_MatchId_Does_Not_Exist_In_Matches_Dictionary()
        {
            // Arrange
            var context = new MessagesTransformationContext { Matches = new Dictionary<string, MessagesTransformationContext.MatchIdProfile>() };

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => context.GetMatchProfile("nonExistingId"));
        }
    }
}
