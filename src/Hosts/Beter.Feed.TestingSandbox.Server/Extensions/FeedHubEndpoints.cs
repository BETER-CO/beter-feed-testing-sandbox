using Beter.Feed.TestingSandbox.Server.SignalR.Hubs.V1;
using Beter.Feed.TestingSandbox.Server.SignalR.Settings;

namespace Beter.Feed.TestingSandbox.Server.Extensions;

public static class FeedHubEndpoints
{
    public const string CorsPolicy = "SignalR";

    public static IEndpointRouteBuilder MapFeedHubs(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
    {
        var settings = configuration.GetSection(SignalRSettings.SECTION_NAME).Get<SignalRSettings>();

        endpoints.MapVersionedHub<TimeTableHub>(settings.FeedTimeTableEndpoint);
        endpoints.MapVersionedHub<ScoreboardHub>(settings.FeedScoreboardEndpoint);
        endpoints.MapVersionedHub<IncidentHub>(settings.FeedIncidentEndpoint);
        endpoints.MapVersionedHub<TradingHub>(settings.FeedTradingEndpoint, static cfg => { cfg.TransportMaxBufferSize = 98304; });

        return endpoints;
    }
}
