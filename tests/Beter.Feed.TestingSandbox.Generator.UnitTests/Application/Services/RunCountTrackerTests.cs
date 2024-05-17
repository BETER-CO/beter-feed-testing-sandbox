using Beter.Feed.TestingSandbox.Generator.Application.Services;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services
{
    public class RunCountTrackerTests
    {
        [Fact]
        public void GetNext_ReturnsIncrementedValue()
        {
            // Act
            var tracker = new RunCountTracker();
            var result1 = tracker.GetNext();
            var result2 = tracker.GetNext();

            // Assert
            Assert.Equal(1, result1);
            Assert.Equal(2, result2);
        }

        [Fact]
        public void GetNext_ThreadSafety_Test()
        {
            // Arrange
            const int expectedRunCount = 100;
            var tracker = new RunCountTracker();

            // Act
            Parallel.For(0, expectedRunCount, _ =>
            {
                tracker.GetNext();
            });

            // Assert
            Assert.Equal(expectedRunCount + 1, tracker.GetNext());
        }
    }
}
