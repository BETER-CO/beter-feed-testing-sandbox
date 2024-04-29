using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models.TradingInfos;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Emulator.Messaging.Handlers.Abstract;

namespace Beter.TestingTools.Emulator.Messaging.Handlers;

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