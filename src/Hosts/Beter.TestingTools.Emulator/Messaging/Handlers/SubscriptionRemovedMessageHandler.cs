using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models;
using Beter.TestingTools.Emulator.Messaging.Handlers.Abstract;
using Beter.TestingTools.Emulator.Publishers;

namespace Beter.TestingTools.Emulator.Messaging.Handlers;

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
