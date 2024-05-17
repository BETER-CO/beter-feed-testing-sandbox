using Beter.Feed.TestingSandbox.Consumer.Clients;
using Microsoft.Extensions.Logging;

namespace Beter.Feed.TestingSandbox.Consumer.Consumers;

internal class ScoreboardConsumer : ConsumerBase<ScoreboardFeedServiceClient>
{
    public ScoreboardConsumer(
        ScoreboardFeedServiceClient scoreboardFeedServiceClient,
        ILogger<ScoreboardConsumer> logger)
        : base(scoreboardFeedServiceClient, logger)
    {
    }
}