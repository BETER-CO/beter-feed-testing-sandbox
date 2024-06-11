using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;

public class IncidentMessageHandler : MessageHandlerBase<IncidentModel[]>
{
    private readonly IMessagePublisher<IncidentModel> _messagePublisher;

    public IncidentMessageHandler(IMessagePublisher<IncidentModel> messagePublisher)
    {
        _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
    }

    public override Task HandleAsync(IncidentModel[] messages, ConsumeMessageContext context, CancellationToken cancellationToken = default)
    {
        return _messagePublisher.GroupPublish(GroupNames.DefaultGroupName, messages, cancellationToken);
    }
}