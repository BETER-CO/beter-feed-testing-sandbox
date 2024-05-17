using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers.Offsets
{
    public sealed class SequenceOffsetTransformStrategy : IOffsetTransformStrategy
    {
        private readonly IOffsetStorage _offsetStorage;

        public SequenceOffsetTransformStrategy(IOffsetStorage offsetStorage)
        {
            _offsetStorage = offsetStorage ?? throw new ArgumentNullException(nameof(offsetStorage));
        }

        public void Transform(TestScenarioMessage message, HubKind hubKind)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            foreach (var messageToModification in message.ToFeedMessages())
            {
                var matchId = messageToModification.Id;
                var msgType = messageToModification.MsgType;

                if (msgType == (int)MessageType.Update)
                {
                    messageToModification.Offset = _offsetStorage.GetOffsetForUpdateMessage(matchId, hubKind);
                }
                else
                {
                    messageToModification.Offset = _offsetStorage.GetOffsetForNonUpdateMessage(matchId, hubKind);
                }
            }
        }
    }
}
