using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Emulator.SignalR.Helpers;
using Beter.Feed.TestingSandbox.Emulator.SignalR.Hubs;

namespace Beter.Feed.TestingSandbox.Emulator.Publishers;

public class FeedMessagePublisher<THub, TModel, THubInterface> : IMessagePublisher<TModel>
    where TModel : class, new()
    where THubInterface : class, IFeedHub<TModel>
    where THub : BaseHub<TModel, THubInterface>
{
    private readonly IHubContext<THub, THubInterface> _hub;
    private readonly ILogger<FeedMessagePublisher<THub, TModel, THubInterface>> _logger;
    public HubKind Hub { get; set; }
    public FeedMessagePublisher(IHubContext<THub, THubInterface> hub, ILogger<FeedMessagePublisher<THub, TModel, THubInterface>> logger)
    {
        _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        _logger = logger ?? NullLogger<FeedMessagePublisher<THub, TModel, THubInterface>>.Instance;
        Hub = HubHelper.ToHub<TModel>();
    }

    public Task SystemGroupPublish(string groupId, IEnumerable<IGlobalMessageModel> items, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing {ModelCount} system events to group {GroupName}", items.Count(), groupId);

        return _hub.Clients.Group(groupId).OnSystemEvent(items, cancellationToken);
    }

    public Task GroupPublish(string defaultGroupName, TModel[] model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing {ModelCount} models to group {GroupName}", model.Length, defaultGroupName);

        return _hub.Clients.Group(defaultGroupName).OnUpdate(model, cancellationToken);
    }

    public Task SendGroupRemoveSubscriptionsAsync(string defaultGroupName, string[] ids, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Subscriptions '{@Ids}' were removed from group {GroupName}.", ids, defaultGroupName);

        return _hub.Clients.Group(defaultGroupName).OnSubscriptionsRemove(ids, cancellationToken);
    }

    public Task SendGroupOnHeartbeatAsync(string defaultGroupName, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending heartbeat to group {GroupName}", defaultGroupName);
        return _hub.Clients.Group(defaultGroupName).OnHeartbeat(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), cancellationToken);
    }

    public Task GroupPublishEmptyArray(string defaultGroupName, CancellationToken cancellationToken)
    {
        return GroupPublish(defaultGroupName, Array.Empty<TModel>(), cancellationToken);
    }
}