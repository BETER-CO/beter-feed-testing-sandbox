using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Common.Enums;
using Beter.TestingTools.Common.Extensions;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Helpers;
using Beter.TestingTools.Generator.Application.Contracts;
using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Services.TestScenarios.MessageHandlers;

public class FeedMessageHandler : BaseTestScenarioMessageHandler
{
    private readonly IOffsetStorage _offsetStorage;

    public FeedMessageHandler(IPublisher publisher, IOffsetStorage offsetStorage) : base(publisher)
    {
        _offsetStorage = offsetStorage ?? throw new ArgumentNullException(nameof(offsetStorage));
    }

    public override bool IsApplicable(string messageType)
    {
        return new[] { MessageTypes.Scoreboard, MessageTypes.Trading, MessageTypes.Incident, MessageTypes.Timetable }.Contains(messageType);
    }

    public override Task BeforePublish(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken)
    {
        var channel = message.Channel;
        var hubKind = HubEnumHelper.ToHub(channel);

        UpdateOffset(message, hubKind);

        return Task.CompletedTask;
    }

    public async override Task AfterPublish(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken)
    {
        var msgType = new FeedMessageWrapper(message.Value.AsArray().First()).MsgType;
        if (msgType == (int)MessageType.ConnectionSnapshot)
        {
            await _publisher.PublishEmptyAsync(message.MessageType, message.Channel, playbackId, cancellationToken);
        }
    }

    private void UpdateOffset(TestScenarioMessage message, HubKind hubKind)
    {
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

