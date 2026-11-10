using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Sinks;
using Beter.Feed.TestingSandbox.Consumer.Utilities;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using MessagePack;
using MessagePack.Resolvers;
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

    private readonly IFeedMessageSink _sink;
    private readonly FeedServiceOptions _feedServiceOptions;
    private readonly ILogger<FeedServiceClientBase<T>> _logger;

    public abstract string Channel { get; }

    public FeedServiceClientBase(
        ILogger<FeedServiceClientBase<T>> logger,
        IOptions<FeedServiceOptions> feedServiceOptions,
        IFeedMessageSink sink)
    {
        _logger = logger ?? NullLogger<FeedServiceClientBase<T>>.Instance;
        _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        _feedServiceOptions = feedServiceOptions.Value ?? throw new ArgumentNullException(nameof(feedServiceOptions));

        connectionUrl = GetConnectionUrl();
        _connection = BuildConnection(connectionUrl);
    }

    /// <summary>
    /// Connects to the hub, retrying every <see cref="FeedServiceOptions.ReconnectionWaitSeconds"/>
    /// until the connection starts or <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        var url = MaskApiKey(connectionUrl);
        var delay = TimeSpan.FromSeconds(_feedServiceOptions.ReconnectionWaitSeconds);
        var attempt = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            attempt++;

            try
            {
                await _connection.StartAsync(cancellationToken);

                _logger.LogInformation($"Connected to URL {url} with connection ID {_connection.ConnectionId}.");
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "Connection attempt {Attempt} to {Url} failed: {Error}. Retrying in {Delay} s.",
                    attempt,
                    url,
                    ex.Message,
                    delay.TotalSeconds);
            }

            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
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
            .WithAutomaticReconnect(new RetryPolicyLoop(_feedServiceOptions.ReconnectionWaitSeconds));

        AddProtocol(connectionBuilder);

        var connection = connectionBuilder.Build();
        connection.Reconnected += ConnectionReconnected;
        connection.Closed += ConnectionClosed;

        connection.On<T[]>(HubMethods.OnUpdate, OnUpdateHandler);
        connection.On<GlobalMessageModel[]>(HubMethods.OnSystemEvent, OnSystemEventHandler);
        connection.On<long>(HubMethods.OnHeartbeat, OnHeartbeatHandler);
        connection.On<string[]>(HubMethods.OnSubscriptionsRemove, OnSubscriptionsRemoveHandler);

        return connection;
    }

    private void AddProtocol(IHubConnectionBuilder connectionBuilder)
    {
        var protocol = _feedServiceOptions.Protocol ?? FeedServiceOptions.JsonProtocol;

        if (string.Equals(protocol, FeedServiceOptions.JsonProtocol, StringComparison.OrdinalIgnoreCase))
        {
            connectionBuilder.AddJsonProtocol(x => x.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
        }
        else if (string.Equals(protocol, FeedServiceOptions.MessagePackProtocol, StringComparison.OrdinalIgnoreCase))
        {
            // Configured the same way as the server.
            connectionBuilder.AddMessagePackProtocol(options =>
                options.SerializerOptions = MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance));
        }
        else
        {
            throw new ArgumentException(
                $"Unknown protocol '{protocol}'. Use '{FeedServiceOptions.JsonProtocol}' or '{FeedServiceOptions.MessagePackProtocol}'.");
        }
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

    private string MaskApiKey(string url)
    {
        var apiKey = _feedServiceOptions.ApiKey;
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 4)
        {
            return url;
        }

        var replacement = new string('X', apiKey.Length - 4);
        return url.Replace(apiKey[4..], replacement);
    }

    private Task ConnectionReconnected(string arg)
    {
        _logger.LogWarning($"Reconnected with arguments {arg}.");
        return Task.CompletedTask;
    }

    private Task OnHeartbeatHandler(long heartbeat)
    {
        return _sink.HandleAsync(Channel, HubMethods.OnHeartbeat, heartbeat);
    }

    private Task OnSubscriptionsRemoveHandler(string[] subscriptions)
    {
        return _sink.HandleAsync(Channel, HubMethods.OnSubscriptionsRemove, subscriptions);
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

    private Task OnUpdateHandler(T[] messages)
    {
        // Empty arrays are passed on too, so an empty snapshot reaches the sink.
        return _sink.HandleAsync(Channel, HubMethods.OnUpdate, messages);
    }

    private Task OnSystemEventHandler(GlobalMessageModel[] messages)
    {
        return _sink.HandleAsync(Channel, HubMethods.OnSystemEvent, messages);
    }
}
