using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Models;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Beter.Feed.TestingSandbox.Consumer.Producers;

public sealed class FeedMessageProducer<TModel> : IFeedMessageProducer<TModel> where TModel : class, IIdentityModel
{
    private readonly FeedServiceOptions _feedServiceOptions;
    private readonly ILogger<FeedMessageProducer<TModel>> _logger;
    private readonly IProducerTransport<FeedMessageModel<IEnumerable<TModel>>> _modelProducerTransport;
    private readonly IProducerTransport<FeedMessageModel<IEnumerable<GlobalMessageModel>>> _globalMessageProducerTransport;
    private readonly IProducerTransport<FeedMessageModel<SubscriptionsRemovedModel>> _subscriptionsRemovedProducerTransport;

    public FeedMessageProducer(
        ILogger<FeedMessageProducer<TModel>> logger,
        IOptions<FeedServiceOptions> feedServiceOptions,
        IProducerTransport<FeedMessageModel<IEnumerable<TModel>>> modelProducerTransport,
        IProducerTransport<FeedMessageModel<IEnumerable<GlobalMessageModel>>> globalMessageProducerTransport,
        IProducerTransport<FeedMessageModel<SubscriptionsRemovedModel>> subscriptionsRemovedProducerTransport)
    {
        _logger = logger ?? NullLogger<FeedMessageProducer<TModel>>.Instance;
        _modelProducerTransport = modelProducerTransport ?? throw new ArgumentNullException(nameof(modelProducerTransport));
        _globalMessageProducerTransport = globalMessageProducerTransport ?? throw new ArgumentNullException(nameof(globalMessageProducerTransport));
        _subscriptionsRemovedProducerTransport = subscriptionsRemovedProducerTransport ?? throw new ArgumentNullException(nameof(subscriptionsRemovedProducerTransport));
        _feedServiceOptions = feedServiceOptions.Value ?? throw new ArgumentNullException(nameof(feedServiceOptions));
    }

    public Task ProduceAsync(IEnumerable<TModel> messages, string channel, CancellationToken cancellationToken = default)
    {
        return ProduceMessageInternalAsync(messages, channel, HubMethods.OnUpdate, _modelProducerTransport, cancellationToken);
    }

    public Task ProduceAsync(IEnumerable<GlobalMessageModel> messages, string channel, CancellationToken cancellationToken = default)
    {
        return ProduceMessageInternalAsync(messages, channel, HubMethods.OnSystemEvent, _globalMessageProducerTransport, cancellationToken);
    }

    public Task ProduceAsync(SubscriptionsRemovedModel message, string channel, CancellationToken cancellationToken = default)
    {
        return ProduceMessageInternalAsync(message, channel, HubMethods.OnSubscriptionsRemove, _subscriptionsRemovedProducerTransport, cancellationToken);
    }

    private async Task ProduceMessageInternalAsync<TMessage>(TMessage message, string channel, string method, IProducerTransport<FeedMessageModel<TMessage>> transport, CancellationToken cancellationToken)
    {
        try
        {
            var deliveryResult = await transport.ProduceAsync(_feedServiceOptions.DestinationTopicName, new FeedMessageModel<TMessage>(message, channel, method), cancellationToken);

            _logger.LogDebug($"Message produced to topic {deliveryResult.Topic} with offset {deliveryResult.Offset} with status {deliveryResult.Status}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Message failed to produce to topic {_feedServiceOptions.DestinationTopicName}.");
            throw;
        }
    }
}
