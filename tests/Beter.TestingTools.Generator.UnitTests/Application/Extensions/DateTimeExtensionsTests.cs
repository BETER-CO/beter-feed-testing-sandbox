using Beter.TestingTools.Generator.Application.Extensions;

namespace Beter.TestingTools.Generator.UnitTests.Application.Extensions
{
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void ToUnixTimeMilliseconds_ReturnsCorrectValue()
        {
            // Arrange
            var dateTime = new DateTime(2022, 4, 25, 12, 30, 45, DateTimeKind.Utc);
            var expectedMilliseconds = new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

            // Act
            var result = dateTime.ToUnixTimeMilliseconds();

            // Assert
            Assert.Equal(expectedMilliseconds, result);
        }
    }
}
