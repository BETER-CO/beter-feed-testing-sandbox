using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Producers;
using Beter.Feed.TestingSandbox.Consumer.Utilities;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace Beter.Feed.TestingSandbox.Consumer.Clients;

public abstract class FeedServiceClientBase<T> : IFeedClient where T : class, IFeedMessage
{
    private readonly string connectionUrl;
    private readonly HubConnection _connection;

    private readonly IFeedMessageProducer<T> _producer;
    private readonly FeedServiceOptions _feedServiceOptions;
    private readonly ILogger<FeedServiceClientBase<T>> _logger;

    public abstract string Channel { get; }

    public FeedServiceClientBase(
        ILogger<FeedServiceClientBase<T>> logger,
        IOptions<FeedServiceOptions> feedServiceOptions,
        IFeedMessageProducer<T> producer)
    {
        _logger = logger ?? NullLogger<FeedServiceClientBase<T>>.Instance;
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _feedServiceOptions = feedServiceOptions.Value ?? throw new ArgumentNullException(nameof(feedServiceOptions));

        connectionUrl = GetConnectionUrl();
        _connection = BuildConnection(connectionUrl);

        _logger.LogInformation("Destination topic name: '{DestinationTopicName}'", _feedServiceOptions.DestinationTopicName);
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _connection.StartAsync(cancellationToken);

            var apiKeyLength = _feedServiceOptions.ApiKey.Length;
            var replacement = new string('X', apiKeyLength - 4);
            var url = connectionUrl.Replace(_feedServiceOptions.ApiKey[4..], replacement);

            _logger.LogInformation($"Connected to URL {url} with connection ID {_connection.ConnectionId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred executing task work item.");
        }
    }

    private HubConnection BuildConnection(string connectionUrl)
    {
        if (string.IsNullOrWhiteSpace(connectionUrl))
        {
            throw new ArgumentNullException(nameof(connectionUrl));
        }

        var connectionBuilder = new HubConnectionBuilder()
            .WithUrl(connectionUrl, options =>
            {
                options.Transports = HttpTransportType.WebSockets;
                options.SkipNegotiation = _feedServiceOptions.SkipNegotiation;
            })
            .AddJsonProtocol(x => x.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)
            .WithAutomaticReconnect(new RetryPolicyLoop(_feedServiceOptions.ReconnectionWaitSeconds));

        var connection = connectionBuilder.Build();
        connection.Reconnected += ConnectionReconnected;
        connection.Closed += ConnectionClosed;

        connection.On<T[]>(HubMethods.OnUpdate, OnUpdateHandler);
        connection.On<GlobalMessageModel[]>(HubMethods.OnSystemEvent, OnSystemEventHandler);
        connection.On<long>(HubMethods.OnHeartbeat, OnHeartbeatHandler);
        connection.On<string[]>(HubMethods.OnSubscriptionsRemove, OnSubscriptionsRemoveHandler);

        return connection;
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        await _connection.StopAsync(cancellationToken);
        await _connection.DisposeAsync();
    }

    private string GetConnectionUrl()
    {
        var result = $"{_feedServiceOptions.Host}/{Channel}?ApiKey={_feedServiceOptions.ApiKey}";

        return _feedServiceOptions.SnapshotBatchSize > 0
            ? $"{result}&snapshotBatchSize={_feedServiceOptions.SnapshotBatchSize}"
            : result;
    }

    private Task ConnectionReconnected(string arg)
    {
        _logger.LogWarning($"Reconnected with arguments {arg}.");
        return Task.CompletedTask;
    }

    private void OnHeartbeatHandler(long heartbeat)
    {
        _logger.LogInformation("Heartbeat");
        _logger.LogInformation(heartbeat.ToString());
    }

    private async Task OnSubscriptionsRemoveHandler(string[] subscriptions)
    {
        var model = new SubscriptionsRemovedModel { Ids = subscriptions };

        await _producer.ProduceAsync(model, Channel);

        _logger.LogInformation(
             "Channel: {Channel}, SubscriptionRemoved produced for messages: {MessageIds}",
             Channel,
             subscriptions.Aggregate((x, y) => $"{x}, {y}"));
    }

    private Task ConnectionClosed(Exception exception)
    {
        _logger.LogWarning($"Disconnected {_connection.ConnectionId}.");

        if (exception != null)
        {
            _logger.LogError(exception.Message);
        }

        return Task.CompletedTask;
    }

    private async Task OnUpdateHandler(T[] messages)
    {
        if (messages.Any())
        {
            await _producer.ProduceAsync(messages, Channel);

            _logger.LogInformation(
                "Channel: {Channel}, Messages produced: {MessageIds}",
                Channel,
                string.Join(',', messages.Select(message => message.Id).Distinct()));
        }
    }

    private async Task OnSystemEventHandler(GlobalMessageModel[] messages)
    {
        await _producer.ProduceAsync(messages, Channel);

        _logger.LogInformation(
           "Channel: {Channel}, System events produced for messages: {MessageIds}",
           Channel,
           string.Join(',', messages.Select(message => message.Id).Distinct()));
    }
}