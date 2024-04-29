using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models.Incidents;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Emulator.Messaging.Handlers.Abstract;

namespace Beter.TestingTools.Emulator.Messaging.Handlers;

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