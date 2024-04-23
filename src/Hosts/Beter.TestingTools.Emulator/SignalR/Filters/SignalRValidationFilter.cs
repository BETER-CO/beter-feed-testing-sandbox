using Beter.TestingTools.Emulator.SignalR.Helpers;
using Beter.TestingTools.Emulator.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Beter.TestingTools.Emulator.SignalR.Filters;

public sealed class SignalRValidationFilter : IHubFilter
{
    private readonly ILogger<SignalRValidationFilter> _logger;

    public SignalRValidationFilter(ILogger<SignalRValidationFilter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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