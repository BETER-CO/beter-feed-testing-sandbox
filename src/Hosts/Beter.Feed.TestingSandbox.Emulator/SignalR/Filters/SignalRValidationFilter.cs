using Beter.Feed.TestingSandbox.Emulator.SignalR.Helpers;
using Beter.Feed.TestingSandbox.Emulator.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;

namespace Beter.Feed.TestingSandbox.Emulator.SignalR.Filters;

public sealed class SignalRValidationFilter : IHubFilter
{
    private readonly ILogger<SignalRValidationFilter> _logger;

    public SignalRValidationFilter(ILogger<SignalRValidationFilter> logger)
    {
        _logger = logger ?? NullLogger<SignalRValidationFilter>.Instance;
    }

    public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        var hub = context.Hub as IHubIdentity;
        var ctx = context.Context;

        _logger.LogInformation("Attempting API key validation for connection ID {ConnectionId} in {HubKind} hub.", ctx.ConnectionId, hub.Hub);

        if (!IsValidApiKey(ctx))
        {
            throw new HubException($"API key validation failed.");
        }

        await next(context);
    }

    private static bool IsValidApiKey(HubCallerContext context) => context.GetKey() != Guid.Empty;
}