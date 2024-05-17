using Beter.Feed.TestingSandbox.Emulator.SignalR.Hubs.V1;
using Beter.Feed.TestingSandbox.Emulator.SignalR.Settings;

namespace Beter.Feed.TestingSandbox.Emulator.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSignalR(this IApplicationBuilder app, IConfiguration configuration)
    {
        var settings = configuration.GetSection(SignalRSettings.SECTION_NAME).Get<SignalRSettings>();

        app.UseCors("SignalR");
        app.UseEndpoints(builder =>
        {
            builder.MapVersionedHub<TimeTableHub>(settings.FeedTimeTableEndpoint);
            builder.MapVersionedHub<ScoreboardHub>(settings.FeedScoreboardEndpoint);
            builder.MapVersionedHub<IncidentHub>(settings.FeedIncidentEndpoint);
            builder.MapVersionedHub<TradingHub>(settings.FeedTradingEndpoint, static cfg => { cfg.TransportMaxBufferSize = 98304; });
        });

        return app;
    }
}
