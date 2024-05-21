using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Common.Extensions;
using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations.Helpers;
using Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers.Offsets;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers;

public class FeedMessageHandler : BaseTestScenarioMessageHandler
{
    private readonly IOffsetTransformStrategyResolver _offsetTransformStrategyResolver;

    public FeedMessageHandler(IPublisher publisher, IOffsetTransformStrategyResolver offsetTransformStrategyResolver) : base(publisher)
    {
        _offsetTransformStrategyResolver = offsetTransformStrategyResolver ?? throw new ArgumentNullException(nameof(offsetTransformStrategyResolver));
    }

    public override bool IsApplicable(string messageType)
    {
        return new[] { MessageTypes.Scoreboard, MessageTypes.Trading, MessageTypes.Incident, MessageTypes.Timetable }.Contains(messageType);
    }

    public override Task BeforePublish(TestScenarioMessage message, Guid playbackId, AdditionalInfo additionalInfo, CancellationToken cancellationToken)
    {
        var channel = message.Channel;
        var hubKind = HubEnumHelper.ToHub(channel);

        _offsetTransformStrategyResolver
            .Resolve(additionalInfo)
            .Transform(message, hubKind);

        return Task.CompletedTask;
    }

    public async override Task AfterPublish(TestScenarioMessage message, Guid playbackId, AdditionalInfo additionalInfo, CancellationToken cancellationToken)
    {
        var msgType = new FeedMessageWrapper(message.Value.AsArray().First()).MsgType;
        if (msgType == (int)MessageType.ConnectionSnapshot)
        {
            await _publisher.PublishEmptyAsync(message.MessageType, message.Channel, playbackId, cancellationToken);
        }
    }
}

