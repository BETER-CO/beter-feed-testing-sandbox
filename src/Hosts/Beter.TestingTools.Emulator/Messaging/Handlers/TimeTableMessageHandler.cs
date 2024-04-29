using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models.TimeTableItems;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Emulator.Messaging.Handlers.Abstract;

namespace Beter.TestingTools.Emulator.Messaging.Handlers;

public class TimeTableMessageHandler : MessageHandlerBase<TimeTableItemModel[]>
{
    private readonly IMessagePublisher<TimeTableItemModel> _messagePublisher;

    public TimeTableMessageHandler(IMessagePublisher<TimeTableItemModel> messagePublisher)
    {
        _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
    }

    public override Task HandleAsync(TimeTableItemModel[] messages, ConsumeMessageContext context, CancellationToken cancellationToken = default)
    {
        return _messagePublisher.GroupPublish(GroupNames.DefaultGroupName, messages, cancellationToken);
    }
}