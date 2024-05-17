using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Producers;
using Beter.Feed.TestingSandbox.Consumer.Services.Abstract;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace Beter.Feed.TestingSandbox.Consumer.Clients;

public class IncidentFeedServiceClient : FeedServiceClientBase<IncidentModel>
{
    public override string Channel => ChannelNames.Incident;

    public IncidentFeedServiceClient(
        ITestScenarioTemplateService templateService,
        IFeatureManager featureManager,
        ILogger<IncidentFeedServiceClient> logger,
        IOptions<FeedServiceOptions> feedServiceOptions,
        IFeedMessageProducer<IncidentModel> producer)
        : base(templateService, featureManager, logger, feedServiceOptions, producer)
    {
    }
}