using Beter.Feed.TestingSandbox.Common.Extensions;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Extensions
{
    public class TaskExtensionsTests
    {
        [Fact]
        public async Task ExecuteWithTimeout_CompletesWithinTimeout()
        {
            // Arrange
            var timeout = TimeSpan.FromSeconds(2);
            var task = Task.Delay(1000);
            bool timeoutOccurred = false;

            // Act
            await task.ExecuteWithTimeout(timeout, () => timeoutOccurred = true);

            // Assert
            Assert.False(timeoutOccurred);
        }

        [Fact]
        public async Task ExecuteWithTimeout_TimesOut()
        {
            // Arrange
            var timeout = TimeSpan.FromMilliseconds(100);
            var task = Task.Delay(1000);
            bool timeoutOccurred = false;

            // Act
            await task.ExecuteWithTimeout(timeout, () => timeoutOccurred = true);

            // Assert
            Assert.True(timeoutOccurred);
        }

        [Fact]
        public async Task ExecuteWithTimeout_ThrowsArgumentNullException_ForNullTask()
        {
            // Arrange
            Task nullTask = null;
            var timeout = TimeSpan.FromSeconds(2);

            //Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => nullTask.ExecuteWithTimeout(timeout, () => { }));
        }
    }
}
