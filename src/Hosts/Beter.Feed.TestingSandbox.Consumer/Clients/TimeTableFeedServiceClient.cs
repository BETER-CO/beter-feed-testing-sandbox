using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Producers;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beter.Feed.TestingSandbox.Consumer.Clients;

public class TimeTableFeedServiceClient : FeedServiceClientBase<TimeTableItemModel>
{
    public override string Channel => ChannelNames.Timetable;

    public TimeTableFeedServiceClient(
        ILogger<FeedServiceClientBase<TimeTableItemModel>> logger,
        IOptions<FeedServiceOptions> feedServiceOptions,
        IFeedMessageProducer<TimeTableItemModel> producer)
        : base(logger, feedServiceOptions, producer)
    {
    }
}