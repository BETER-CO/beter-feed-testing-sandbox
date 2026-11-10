using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Sinks;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beter.Feed.TestingSandbox.Consumer.Clients;

public class IncidentFeedServiceClient : FeedServiceClientBase<IncidentModel>
{
    public override string Channel => ChannelNames.Incident;

    public IncidentFeedServiceClient(
        ILogger<IncidentFeedServiceClient> logger,
        IOptions<FeedServiceOptions> feedServiceOptions,
        IFeedMessageSink sink)
        : base(logger, feedServiceOptions, sink)
    {
    }
}