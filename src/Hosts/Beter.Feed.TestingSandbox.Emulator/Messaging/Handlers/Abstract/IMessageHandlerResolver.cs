using Beter.Feed.TestingSandbox.Emulator.Messaging;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;

public interface IMessageHandlerResolver
{
    IMessageHandler Resolve(ConsumeMessageContext context);
}