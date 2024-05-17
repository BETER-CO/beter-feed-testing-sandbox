using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts;
using System.Collections.Concurrent;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers.Offsets
{
    public sealed class OffsetStorage : IOffsetStorage
    {
        private readonly ISequenceNumberProvider _sequenceNumberProvider;
        private readonly ConcurrentDictionary<string, Dictionary<HubKind, int>> _offsetStorage;

        public OffsetStorage(ISequenceNumberProvider sequenceNumberProvider)
        {
            _offsetStorage = new ConcurrentDictionary<string, Dictionary<HubKind, int>>();
            _sequenceNumberProvider = sequenceNumberProvider ?? throw new ArgumentNullException(nameof(sequenceNumberProvider));
        }

        public int GetOffsetForNonUpdateMessage(string matchId, HubKind hubKind)
        {
            var offsetByHubs = _offsetStorage.AddOrUpdate(
                matchId,
                (matchId) => new Dictionary<HubKind, int> { { hubKind, _sequenceNumberProvider.GetNext() } },
                (matchId, offsetByHubs) =>
                {
                    if (!offsetByHubs.TryGetValue(hubKind, out var offset))
                        offsetByHubs[hubKind] = _sequenceNumberProvider.GetNext();

                    return offsetByHubs;
                });

            return offsetByHubs[hubKind];
        }

        public int GetOffsetForUpdateMessage(string matchId, HubKind hubKind)
        {
            var newOffset = _sequenceNumberProvider.GetNext();

            _offsetStorage.AddOrUpdate(
                matchId,
                (matchId) => new Dictionary<HubKind, int> { { hubKind, newOffset } },
                (matchId, offsetByHubs) =>
                {
                    offsetByHubs[hubKind] = newOffset;
                    return offsetByHubs;
                });

            return newOffset;
        }
    }
}
