using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Common.Extensions;
using Beter.Feed.TestingSandbox.Common.Serialization;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Producers;
using Beter.Feed.TestingSandbox.Consumer.Services;
using Beter.Feed.TestingSandbox.Consumer.Services.Abstract;
using Beter.Feed.TestingSandbox.Consumer.Utilities;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using System.Text.Json.Serialization;

namespace Beter.Feed.TestingSandbox.Consumer.Clients;

public abstract class FeedServiceClientBase<T> : IFeedClient where T : class, IFeedMessage
{
    private readonly string connectionUrl;
    private readonly HubConnection _connection;

    private readonly IFeatureManager _featureManager;
    private readonly ITestScenarioTemplateService _templateService;
    private readonly IFeedMessageProducer<T> _producer;
    private readonly FeedServiceOptions _feedServiceOptions;
    private readonly ILogger<FeedServiceClientBase<T>> _logger;
    private readonly SemaphoreSlim _semaphore;

    public abstract string Channel { get; }

    public FeedServiceClientBase(
        ITestScenarioTemplateService templateService,
        IFeatureManager featureManager,
        ILogger<FeedServiceClientBase<T>> logger,
        IOptions<FeedServiceOptions> feedServiceOptions,
        IFeedMessageProducer<T> producer)
    {
        _semaphore = new SemaphoreSlim(1, 1);
        _logger = logger ?? NullLogger<FeedServiceClientBase<T>>.Instance;
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _feedServiceOptions = feedServiceOptions.Value ?? throw new ArgumentNullException(nameof(feedServiceOptions));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));

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

        await EnforceThatGotExpectedFeedMessage(model);
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
            await EnforceThatGotExpectedFeedMessage(messages);
            await _producer.ProduceAsync(messages, Channel);

            _logger.LogInformation(
                "Channel: {Channel}, Messages produced: {MessageIds}",
                Channel,
                string.Join(',', messages.Select(message => message.Id).Distinct()));
        }
    }

    private async Task OnSystemEventHandler(GlobalMessageModel[] messages)
    {
        await EnforceThatGotExpectedFeedMessage(messages);
        await _producer.ProduceAsync(messages, Channel);

        _logger.LogInformation(
           "Channel: {Channel}, System events produced for messages: {MessageIds}",
           Channel,
           string.Join(',', messages.Select(message => message.Id).Distinct()));
    }

    private async Task EnforceThatGotExpectedFeedMessage<TValue>(TValue actualMessage) where TValue : class
    {
        if (await _featureManager.IsEnabledAsync(FeatureManagementFlags.ConsumeByTemplate))
        {
            await _semaphore.WaitAsync();

            var hubKind = HubEnumHelper.ToHub(Channel);
            var nextItem = _templateService.GetNext(hubKind);
            var expectedMessage = JsonHubSerializer.Deserialize<TValue>(nextItem);

            if (!AreMessagesEqual(expectedMessage, actualMessage))
            {
                LogMessageMismatch(expectedMessage, actualMessage, hubKind);
            }

            LogTemplateProcessedStatus();

            _semaphore.Release();
        }
    }

    private static bool AreMessagesEqual<TValue>(TValue expectedMessage, TValue actualMessage) where TValue : class
    {
        try
        {
            return TestScenarioMessagesComparer.Compare(expectedMessage, actualMessage);
        }
        catch
        {
            return false;
        }
    }

    private void LogMessageMismatch<TValue>(TValue expectedMessage, TValue actualMessage, HubKind hubKind)
    {
        var actual = JsonHubSerializer.Serialize(actualMessage);
        var expected = JsonHubSerializer.Serialize(expectedMessage);

        _templateService.SetMissmatchItem(hubKind, expected, actual);

        _logger.LogError($"It is wrong sequence of messages.");
    }

    private void LogTemplateProcessedStatus()
    {
        if (!_templateService.GetTemplate().IsProcessed)
        {
            _logger.LogInformation($"Successfully processed.");
        }
    }
}