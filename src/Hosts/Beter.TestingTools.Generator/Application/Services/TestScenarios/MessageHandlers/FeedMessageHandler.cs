using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Common.Enums;
using Beter.TestingTools.Common.Extensions;
using Beter.TestingTool.Generator.Application.Contracts;
using Beter.TestingTool.Generator.Application.Services.Playbacks.Transformations.Helpers;
using Beter.TestingTool.Generator.Domain.TestScenarios;
using System.Collections.Concurrent;

namespace Beter.TestingTool.Generator.Application.Services.TestScenarios.MessageHandlers;

public class FeedMessageHandler : BaseTestScenarioMessageHandler
{
    private readonly ISequenceNumberProvider _sequenceNumberProvider;
    private readonly ConcurrentDictionary<string, Dictionary<HubKind, int>> _offsetStorage;

    public FeedMessageHandler(IPublisher publisher, ISequenceNumberProvider sequenceNumberProvider) : base(publisher)
    {
        _offsetStorage = new ConcurrentDictionary<string, Dictionary<HubKind, int>>();
        _sequenceNumberProvider = sequenceNumberProvider ?? throw new ArgumentNullException(nameof(sequenceNumberProvider));
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
                UpdateOffsetForUpdateMessage(messageToModification, matchId, hubKind);
            }
            else
            {
                UpdateOffsetForNonUpdateMessage(messageToModification, matchId, hubKind);
            }
        }
    }

    private void UpdateOffsetForUpdateMessage(FeedMessageWrapper messageToModification, string matchId, HubKind hubKind)
    {
        var newOffset = _sequenceNumberProvider.GetNext();

        _offsetStorage.AddOrUpdate(matchId,
            (matchId) => new Dictionary<HubKind, int> { { hubKind, newOffset } },
            (matchId, offsetByHubs) =>
            {
                offsetByHubs[hubKind] = newOffset;
                return offsetByHubs;
            });

        messageToModification.Offset = newOffset;
    }

    private void UpdateOffsetForNonUpdateMessage(FeedMessageWrapper messageToModification, string matchId, HubKind hubKind)
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

        messageToModification.Offset = offsetByHubs[hubKind];
    }
}

