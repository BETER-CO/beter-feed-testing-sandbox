using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;

public class ScoreboardMessageHandler : MessageHandlerBase<ScoreBoardModel[]>
{
    private readonly IMessagePublisher<ScoreBoardModel> _messagePublisher;

    public ScoreboardMessageHandler(IMessagePublisher<ScoreBoardModel> messagePublisher)
    {
        _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
    }

    public override Task HandleAsync(ScoreBoardModel[] messages, ConsumeMessageContext context, CancellationToken cancellationToken = default)
    {
        return _messagePublisher.GroupPublish(GroupNames.DefaultGroupName, messages, cancellationToken);
    }
}