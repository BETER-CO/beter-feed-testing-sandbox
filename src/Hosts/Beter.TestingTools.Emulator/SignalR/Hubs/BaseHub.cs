using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Common.Enums;
using Beter.TestingTools.Common.Models;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Emulator.Services;
using Beter.TestingTools.Emulator.SignalR.Helpers;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.SignalR;

namespace Beter.TestingTools.Emulator.SignalR.Hubs;

public class BaseHub<T, THubInterface> : Hub<THubInterface>, IHubIdentity where T : class, new()
    where THubInterface : class
{
    private readonly IMessagePublisher<T> _publisher;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<BaseHub<T, THubInterface>> _logger;
    private readonly HubKind _hubKind;
    protected BaseHub(IMessagePublisher<T> publisher, IConnectionManager connectionManager, ILogger<BaseHub<T, THubInterface>> logger)
    {
        _hubKind = HubHelper.ToHub<T>();
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger;
    }

    public HubKind Hub => HubHelper.ToHub<T>();

    public HubVersion Version => HubVersion.V1;

    public async override Task OnConnectedAsync()
    {
        try
        {
            _connectionManager.Set(FeedConnection.Create(Context.ConnectionId, _hubKind.ToString(), DateTime.UtcNow, Context.GetIp()));

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.DefaultGroupName);

            await HeartbeatAsync();

            _logger.LogInformation("Connected to hub {ConnectionId}", Context.ConnectionId);

            await _publisher.GroupPublishEmptyArray(GroupNames.DefaultGroupName, default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection error");
        }


        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        _connectionManager.Remove(Context.ConnectionId);

        _logger.LogInformation($"Disconnected: {exception}");

        return base.OnDisconnectedAsync(exception);
    }

    protected virtual Task OnHeartbeatAsync(HubCallerContext context, string connectionId, Guid apiKey)
    {
        try
        {
            if (_connectionManager.IsActive(connectionId))
                return Task.CompletedTask;

            _logger.LogInformation("Connection aborted '{ConnectionId}'. Because it was found in block list",
                connectionId);

            context.Abort();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured in heartbeat process for connection: '{ConnectionId}'.", connectionId);
            context.Abort();
        }

        return Task.CompletedTask;
    }

    private Task HeartbeatAsync()
    {
        try
        {
            var heartbeat = Context.Features.Get<IConnectionHeartbeatFeature>();

            heartbeat.OnHeartbeat(async state =>
            {

                var (context, connectionId, apiKey) = ((HubCallerContext, string, Guid))state;

                await OnHeartbeatAsync(context, connectionId, apiKey);

            }, (Context, Context.ConnectionId, Context.GetKey()));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to start heartbeat process for connection: '{ConnectionId}'.",
                Context.ConnectionId);
            Context.Abort();
        }

        return Task.CompletedTask;
    }
}
