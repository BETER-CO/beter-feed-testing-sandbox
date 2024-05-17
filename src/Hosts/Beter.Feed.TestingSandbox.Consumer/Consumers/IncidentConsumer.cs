using Beter.Feed.TestingSandbox.Consumer.Clients;
using Microsoft.Extensions.Logging;

namespace Beter.Feed.TestingSandbox.Consumer.Consumers;

internal class IncidentConsumer : ConsumerBase<IncidentFeedServiceClient>
{
    public IncidentConsumer(
        IncidentFeedServiceClient incidentFeedServiceClient,
        ILogger<IncidentConsumer> logger)
        : base(incidentFeedServiceClient, logger)
    {
    }
}