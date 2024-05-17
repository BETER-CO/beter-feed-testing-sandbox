using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Producers;
using Beter.Feed.TestingSandbox.Consumer.Services.Abstract;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace Beter.Feed.TestingSandbox.Consumer.Clients;

public class TimeTableFeedServiceClient : FeedServiceClientBase<TimeTableItemModel>
{
    public override string Channel => ChannelNames.Timetable;

    public TimeTableFeedServiceClient(
        ITestScenarioTemplateService templateService,
        IFeatureManager featureManager,
        ILogger<FeedServiceClientBase<TimeTableItemModel>> logger,
        IOptions<FeedServiceOptions> feedServiceOptions,
        IFeedMessageProducer<TimeTableItemModel> producer)
        : base(templateService, featureManager, logger, feedServiceOptions, producer)
    {
    }
}