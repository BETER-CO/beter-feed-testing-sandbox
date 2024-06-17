using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;

public sealed class SubscriptionRemovedMessageHandler : MessageHandlerBase<SubscriptionsRemovedModel>
{
    private readonly IFeedMessagePublisherResolver _publisherResolver;

    public SubscriptionRemovedMessageHandler(IFeedMessagePublisherResolver publisherResolver)
    {
        _publisherResolver = publisherResolver ?? throw new ArgumentNullException(nameof(publisherResolver));
    }

    public override async Task HandleAsync(SubscriptionsRemovedModel message, ConsumeMessageContext context, CancellationToken cancellationToken = default)
    {
        if (!context.MessageHeaders.TryGetValue(HeaderNames.MessageChannel, out var channel))
        {
            throw new InvalidOperationException("Message has to have channel header.");
        }

        await _publisherResolver
            .Resolve(channel)
            .SendGroupRemoveSubscriptionsAsync(GroupNames.DefaultGroupName, message.Ids.ToArray(), cancellationToken);
    }
}
