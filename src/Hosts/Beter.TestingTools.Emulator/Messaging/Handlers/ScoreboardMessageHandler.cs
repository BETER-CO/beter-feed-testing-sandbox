using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models.Scoreboards;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Emulator.Messaging.Handlers.Abstract;

namespace Beter.TestingTools.Emulator.Messaging.Handlers;

public class ScoreboardMessageHandler : MessageHandlerBase<ScoreBoardModel[]>
{
    private readonly IMessagePublisher<ScoreBoardModel> _messagePublisher;

    public ScoreboardMessageHandler(IMessagePublisher<ScoreBoardModel> messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public override Task HandleAsync(ScoreBoardModel[] messages, ConsumeMessageContext context, CancellationToken cancellationToken = default)
    {
        return _messagePublisher.GroupPublish(GroupNames.DefaultGroupName, messages, cancellationToken);
    }
}