using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;

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