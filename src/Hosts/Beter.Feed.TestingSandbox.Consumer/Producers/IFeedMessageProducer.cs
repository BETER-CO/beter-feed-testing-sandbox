using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;

namespace Beter.Feed.TestingSandbox.Consumer.Producers;

public interface IFeedMessageProducer<TMessage> where TMessage : class, IIdentityModel
{
    Task ProduceAsync(IEnumerable<TMessage> messages, string channel, CancellationToken cancellationToken = default);
    Task ProduceAsync(IEnumerable<GlobalMessageModel> messages, string channel, CancellationToken cancellationToken = default);
    Task ProduceAsync(SubscriptionsRemovedModel message, string channel, CancellationToken cancellationToken = default);
}

