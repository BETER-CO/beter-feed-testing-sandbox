using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models.GlobalEvents;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Emulator.Messaging.Handlers.Abstract;

namespace Beter.TestingTools.Emulator.Messaging.Handlers;

public sealed class GlobalMessageHandler : MessageHandlerBase<GlobalMessageModel[]>
{
    private readonly IFeedMessagePublisherResolver _publisherResolver;

    public GlobalMessageHandler(IFeedMessagePublisherResolver publisherResolver)
    {
        _publisherResolver = publisherResolver ?? throw new ArgumentNullException(nameof(publisherResolver));
    }

    public override async Task HandleAsync(GlobalMessageModel[] globalEvents, ConsumeMessageContext context, CancellationToken cancellationToken = default)
    {
        if (!context.MessageHeaders.TryGetValue(HeaderNames.MessageChannel, out var channel))
        {
            throw new InvalidOperationException("Message has to have channel header.");
        }

        await _publisherResolver
            .Resolve(channel)
            .SystemGroupPublish(GroupNames.DefaultGroupName, globalEvents, cancellationToken);
    }
}

