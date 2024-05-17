using Beter.Feed.TestingSandbox.Consumer.Clients;
using Microsoft.Extensions.Logging;

namespace Beter.Feed.TestingSandbox.Consumer.Consumers;

internal class TimeTableConsumer : ConsumerBase<TimeTableFeedServiceClient>
{
    public TimeTableConsumer(
        TimeTableFeedServiceClient timeTableFeedServiceClient,
        ILogger<TimeTableConsumer> logger)
        : base(timeTableFeedServiceClient, logger)
    {
    }
}
