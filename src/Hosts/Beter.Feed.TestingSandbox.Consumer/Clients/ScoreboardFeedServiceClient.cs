using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Producers;
using Beter.Feed.TestingSandbox.Consumer.Services.Abstract;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace Beter.Feed.TestingSandbox.Consumer.Clients;

public class ScoreboardFeedServiceClient : FeedServiceClientBase<ScoreBoardModel>
{
    public override string Channel => ChannelNames.Scoreboard;

    public ScoreboardFeedServiceClient(
        ITestScenarioTemplateService templateService,
        IFeatureManager featureManager,
        ILogger<FeedServiceClientBase<ScoreBoardModel>> logger,
        IOptions<FeedServiceOptions> feedServiceOptions,
        IFeedMessageProducer<ScoreBoardModel> producer)
        : base(templateService, featureManager, logger, feedServiceOptions, producer)
    {
    }
}