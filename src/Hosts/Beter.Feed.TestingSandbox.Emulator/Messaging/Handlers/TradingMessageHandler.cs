using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;

public class TradingMessageHandler : MessageHandlerBase<TradingInfoModel[]>
{
    private readonly IMessagePublisher<TradingInfoModel> _messagePublisher;

    public TradingMessageHandler(IMessagePublisher<TradingInfoModel> messagePublisher)
    {
        _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
    }

    public override Task HandleAsync(TradingInfoModel[] messages, ConsumeMessageContext context, CancellationToken cancellationToken = default)
    {
        return _messagePublisher.GroupPublish(GroupNames.DefaultGroupName, messages, cancellationToken);
    }
}