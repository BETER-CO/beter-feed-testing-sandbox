using Beter.Feed.TestingSandbox.Consumer.Clients;
using Microsoft.Extensions.Logging;

namespace Beter.Feed.TestingSandbox.Consumer.Consumers;

internal class TradingConsumer : ConsumerBase<TradingFeedServiceClient>
{
    public TradingConsumer(
        TradingFeedServiceClient tradingFeedServiceClient,
        ILogger<TradingConsumer> logger)
        : base(tradingFeedServiceClient, logger)
    {
    }
}