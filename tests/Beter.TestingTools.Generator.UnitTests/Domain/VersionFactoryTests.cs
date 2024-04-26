using Beter.TestingTools.Generator.Domain;

namespace Beter.TestingTools.Generator.UnitTests.Domain
{
    public class VersionFactoryTests
    {
        [Fact]
        public void CreateVersion_ReturnsVersionFromString()
        {
            // Arrange
            var versionString = "1.2.3";

            // Act
            var version = VersionFactory.CreateVersion(versionString);

            // Assert
            Assert.NotNull(version);
            Assert.Equal(new Version(1, 2, 3), version);
        }

        [Fact]
        public void CreateVersion_ReturnsNull_WhenVersionStringIsInvalid()
        {
            // Arrange
            var invalidVersionString = "invalid";

            // Act
            var version = VersionFactory.CreateVersion(invalidVersionString);

            // Assert
            Assert.Null(version);
        }
    }
}
