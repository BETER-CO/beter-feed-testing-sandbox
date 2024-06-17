using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;

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

