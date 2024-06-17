using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Producers;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beter.Feed.TestingSandbox.Consumer.Clients;

public class ScoreboardFeedServiceClient : FeedServiceClientBase<ScoreBoardModel>
{
    public override string Channel => ChannelNames.Scoreboard;

    public ScoreboardFeedServiceClient(
        ILogger<FeedServiceClientBase<ScoreBoardModel>> logger,
        IOptions<FeedServiceOptions> feedServiceOptions,
        IFeedMessageProducer<ScoreBoardModel> producer)
        : base(logger, feedServiceOptions, producer)
    {
    }
}