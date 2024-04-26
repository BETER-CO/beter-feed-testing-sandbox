using Beter.TestingTools.Generator.Application.Extensions;

namespace Beter.TestingTools.Generator.UnitTests.Application.Extensions
{
    public class TimestampExtensionsTests
    {
        [Fact]
        public void ToUtcDateTime_ConvertsTimestampToUtcDateTime()
        {
            // Arrange
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Act
            var utcDateTime = timestamp.ToUtcDateTime();

            // Assert
            Assert.Equal(DateTimeKind.Utc, utcDateTime.Kind);
            Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime, utcDateTime);
        }
    }
}
