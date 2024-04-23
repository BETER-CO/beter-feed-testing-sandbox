namespace Beter.TestingTools.Emulator.Messaging.Handlers.Abstract;

public interface IMessageHandlerResolver
{
    IMessageHandler Resolve(ConsumeMessageContext context);
}