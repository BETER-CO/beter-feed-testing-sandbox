namespace Beter.TestingTools.Emulator.Messaging.Handlers.Abstract;

public interface IMessageHandler
{
    bool IsApplicable(ConsumeMessageContext context);

    Task HandleAsync(ConsumeMessageContext context, CancellationToken cancellationToken);
}

public interface IMessageHandler<TValue> : IMessageHandler
{
    Task HandleAsync(TValue value, ConsumeMessageContext context, CancellationToken cancellationToken);
}