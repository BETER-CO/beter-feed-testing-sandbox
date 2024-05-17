using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;

public class MessageHandlerResolver : IMessageHandlerResolver
{
    private readonly IEnumerable<IMessageHandler> _messageHandlers;

    public MessageHandlerResolver(IEnumerable<IMessageHandler> messageHandlers)
    {
        _messageHandlers = messageHandlers ?? throw new ArgumentNullException(nameof(messageHandlers));
    }

    public IMessageHandler Resolve(ConsumeMessageContext context)
    {
        var handler = _messageHandlers.FirstOrDefault(handler => handler.IsApplicable(context));
        if (handler == null)
        {
            throw new InvalidOperationException($"Unsupported message type {context.MessageType}.");
        }

        return handler;
    }
}


