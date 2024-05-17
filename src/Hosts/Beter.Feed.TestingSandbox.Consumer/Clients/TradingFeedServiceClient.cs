using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Producers;
using Beter.Feed.TestingSandbox.Consumer.Services.Abstract;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace Beter.Feed.TestingSandbox.Consumer.Clients;

public class TradingFeedServiceClient : FeedServiceClientBase<TradingInfoModel>
{
    public override string Channel => ChannelNames.Trading;

    public TradingFeedServiceClient(
        ITestScenarioTemplateService templateService,
        IFeatureManager featureManager,
        ILogger<FeedServiceClientBase<TradingInfoModel>> logger,
        IOptions<FeedServiceOptions> feedServiceOptions,
        IFeedMessageProducer<TradingInfoModel> producer)
        : base(templateService, featureManager, logger, feedServiceOptions, producer)
    {
    }
}