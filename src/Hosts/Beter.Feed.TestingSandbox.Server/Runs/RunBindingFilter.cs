#nullable enable

using Beter.Feed.TestingSandbox.Server.SignalR.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>
/// Binds every hub connection to the runs of its feed <c>ApiKey</c>: the connection joins the
/// group of its key before the hub sends its empty snapshot, so a client that has the snapshot
/// receives every later message of the key; once the hub has accepted the connection, the
/// registry counts it. A closed connection is counted off; no run closes one.
/// </summary>
internal sealed class RunBindingFilter : IHubFilter
{
    private static readonly object KeyItem = new();

    private readonly RunRegistry _registry;

    /// <summary>Initializes a new instance of the <see cref="RunBindingFilter"/> class.</summary>
    /// <param name="registry">The runs.</param>
    public RunBindingFilter(RunRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <inheritdoc />
    public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        var apiKey = context.Context.GetKey();
        if (apiKey == Guid.Empty)
        {
            await next(context);
            return;
        }

        await context.Hub.Groups.AddToGroupAsync(context.Context.ConnectionId, RunGroups.Of(apiKey), context.Context.ConnectionAborted);
        await next(context);
        context.Context.Items[KeyItem] = apiKey;
        _registry.ConnectionJoined(apiKey);
    }

    /// <inheritdoc />
    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        if (context.Context.Items.Remove(KeyItem, out var apiKey) && apiKey is Guid key)
        {
            _registry.ConnectionLeft(key);
        }

        return next(context, exception);
    }
}
