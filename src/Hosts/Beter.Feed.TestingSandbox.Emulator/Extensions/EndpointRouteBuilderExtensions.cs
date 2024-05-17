using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;

namespace Beter.Feed.TestingSandbox.Emulator.Extensions;

static internal class EndpointRouteBuilderExtensions
{
    public const string HubVersionPrefix = "/{version}";

    public static IEndpointRouteBuilder MapVersionedHub<T>(
        this IEndpointRouteBuilder builder,
        string hubUrl,
        Action<HttpConnectionDispatcherOptions> configure = null)
        where T : Hub
    {
        builder.MapHub<T>(string.Concat(HubVersionPrefix, hubUrl), configure);
        builder.MapHub<T>(hubUrl, configure);
        return builder;
    }
}