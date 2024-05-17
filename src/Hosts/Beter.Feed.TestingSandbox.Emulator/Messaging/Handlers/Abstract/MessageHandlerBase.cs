using Beter.Feed.TestingSandbox.Emulator.Messaging;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;

public abstract class MessageHandlerBase<TValue> : IMessageHandler<TValue>
{
    public Task HandleAsync(ConsumeMessageContext context, CancellationToken cancellationToken)
    {
        return HandleAsync((TValue)context.MessageObject, context, cancellationToken);
    }

    public abstract Task HandleAsync(TValue value, ConsumeMessageContext context, CancellationToken cancellationToken);

    public bool IsApplicable(ConsumeMessageContext context) => context.MessageType == typeof(TValue);
}