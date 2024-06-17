using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Producers;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beter.Feed.TestingSandbox.Consumer.Clients;

public class TradingFeedServiceClient : FeedServiceClientBase<TradingInfoModel>
{
    public override string Channel => ChannelNames.Trading;

    public TradingFeedServiceClient(
        ILogger<FeedServiceClientBase<TradingInfoModel>> logger,
        IOptions<FeedServiceOptions> feedServiceOptions,
        IFeedMessageProducer<TradingInfoModel> producer)
        : base(logger, feedServiceOptions, producer)
    {
    }
}