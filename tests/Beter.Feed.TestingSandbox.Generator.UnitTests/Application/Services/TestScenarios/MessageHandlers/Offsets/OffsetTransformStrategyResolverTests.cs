using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers.Offsets;
using Moq;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.TestScenarios.MessageHandlers.Offsets
{
    public class OffsetTransformStrategyResolverTests
    {
        private readonly IOffsetTransformStrategy _customOffsetTransformerStrategy;
        private readonly IOffsetTransformStrategy _sequenceOffsetTransformStrategy;
        private readonly OffsetTransformStrategyResolver _resolver;

        public OffsetTransformStrategyResolverTests()
        {
            _customOffsetTransformerStrategy = new CustomOffsetTransformerStrategy();
            _sequenceOffsetTransformStrategy = new SequenceOffsetTransformStrategy(new Mock<IOffsetStorage>().Object);

            var strategies = new List<IOffsetTransformStrategy>
            {
                _customOffsetTransformerStrategy,
                _sequenceOffsetTransformStrategy
            };

            _resolver = new OffsetTransformStrategyResolver(strategies);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenStrategiesIsNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new OffsetTransformStrategyResolver(null));
        }

        [Fact]
        public void Resolve_ShouldThrowArgumentNullException_WhenAdditionInfoIsNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => _resolver.Resolve(null));
        }

        [Fact]
        public void Resolve_ShouldReturnCustomOffsetTransformerStrategy_WhenIsCustomOffsetIsTrue()
        {
            // Arrange
            var additionalInfo = new AdditionalInfo();
            additionalInfo[AdditionInfoKeys.IsCustomOffset] = true;

            // Act
            var strategy = _resolver.Resolve(additionalInfo);

            // Assert
            Assert.IsType<CustomOffsetTransformerStrategy>(strategy);
        }

        [Fact]
        public void Resolve_ShouldReturnSequenceOffsetTransformStrategy_WhenIsCustomOffsetIsFalse()
        {
            // Arrange
            var additionalInfo = new AdditionalInfo();
            additionalInfo[AdditionInfoKeys.IsCustomOffset] = false;

            // Act
            var strategy = _resolver.Resolve(additionalInfo);

            // Assert
            Assert.IsType<SequenceOffsetTransformStrategy>(strategy);
        }

        [Fact]
        public void Resolve_ShouldThrowInvalidOperationException_WhenCustomOffsetStrategyNotFound()
        {
            // Arrange
            var additionalInfo = new AdditionalInfo();
            additionalInfo[AdditionInfoKeys.IsCustomOffset] = true;

            var resolverWithoutCustomStrategy = new OffsetTransformStrategyResolver(new List<IOffsetTransformStrategy>
            {
                _sequenceOffsetTransformStrategy
            });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => resolverWithoutCustomStrategy.Resolve(additionalInfo));
        }

        [Fact]
        public void Resolve_ShouldThrowInvalidOperationException_WhenSequenceOffsetStrategyNotFound()
        {
            // Arrange
            var additionalInfo = new AdditionalInfo();
            additionalInfo[AdditionInfoKeys.IsCustomOffset] = false;

            var resolverWithoutSequenceStrategy = new OffsetTransformStrategyResolver(new List<IOffsetTransformStrategy>
            {
                _customOffsetTransformerStrategy
            });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => resolverWithoutSequenceStrategy.Resolve(additionalInfo));
        }
    }
}
