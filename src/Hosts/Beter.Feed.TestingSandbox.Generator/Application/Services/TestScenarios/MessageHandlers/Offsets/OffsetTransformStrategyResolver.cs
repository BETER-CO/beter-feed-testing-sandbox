using Beter.Feed.TestingSandbox.Common.Models;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers.Offsets
{
    public sealed class OffsetTransformStrategyResolver : IOffsetTransformStrategyResolver
    {
        private readonly IEnumerable<IOffsetTransformStrategy> _strategies;

        public OffsetTransformStrategyResolver(IEnumerable<IOffsetTransformStrategy> strategies)
        {
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
        }

        public IOffsetTransformStrategy Resolve(AdditionalInfo additionalInfo)
        {
            ArgumentNullException.ThrowIfNull(additionalInfo, nameof(additionalInfo));

            if (additionalInfo.TryGetValue<bool>(AdditionInfoKeys.IsCustomOffset, out var isCustomOffset) && isCustomOffset)
                return RequireStrategy<CustomOffsetTransformerStrategy>("Custom offset transform strategy was not found.");
            else
                return RequireStrategy<SequenceOffsetTransformStrategy>("Sequence offset transform strategy was not found.");
        }

        private IOffsetTransformStrategy RequireStrategy<TStrategy>(string errorMessage) where TStrategy : IOffsetTransformStrategy
        {
            var strategy = _strategies.FirstOrDefault(s => s.GetType() == typeof(TStrategy));
            if (strategy is null)
            {
                throw new InvalidOperationException(errorMessage);
            }

            return strategy;
        }
    }
}
