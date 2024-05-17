using Beter.Feed.TestingSandbox.Generator.Application.Services;
using System.Collections.Concurrent;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services
{
    public class SequenceNumberProviderTests
    {
        [Fact]
        public void GetNext_ReturnsIncrementedNumber()
        {
            // Arrange
            var sequenceNumberProvider = new SequenceNumberProvider();

            // Act
            var firstNumber = sequenceNumberProvider.GetNext();
            var secondNumber = sequenceNumberProvider.GetNext();

            // Assert
            Assert.Equal(1, firstNumber);
            Assert.Equal(2, secondNumber);
        }

        [Fact]
        public void GetNext_ThreadSafety()
        {
            // Arrange
            var sequenceNumberProvider = new SequenceNumberProvider();
            var numIterations = 1000;
            var tasks = new Task[numIterations];
            var result = new ConcurrentBag<int>();

            // Act
            for (int i = 0; i < numIterations; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var nextNumber = sequenceNumberProvider.GetNext();
                    result.Add(nextNumber);
                });
            }

            Task.WaitAll(tasks);

            // Assert
            Assert.Equal(numIterations, result.Count);
            Assert.Equal(numIterations, result.Distinct().Count());
        }
    }
}
