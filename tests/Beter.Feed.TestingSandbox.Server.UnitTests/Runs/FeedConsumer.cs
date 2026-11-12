using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Runs;

/// <summary>
/// A feed client on the four hubs of a test host with one <c>ApiKey</c>, recording every
/// message it receives per hub.
/// </summary>
internal sealed class FeedConsumer : IAsyncDisposable
{
    public static readonly string[] Channels = [ChannelNames.Timetable, ChannelNames.Trading, ChannelNames.Scoreboard, ChannelNames.Incident];

    private readonly Dictionary<string, HubConnection> _connections = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ConcurrentQueue<JsonElement>> _updates = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ConcurrentQueue<JsonElement>> _systemEvents = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<long> _heartbeats = new();

    private FeedConsumer()
    {
    }

    /// <summary>Gets the server send times, in Unix milliseconds, of the heartbeats of the trading hub.</summary>
    public IReadOnlyList<long> TradingHeartbeats => _heartbeats.ToArray();

    /// <summary>Connects on the four hubs and waits for the empty snapshot of each.</summary>
    public static async Task<FeedConsumer> ConnectAsync(SandboxHost host, Guid apiKey)
    {
        var consumer = new FeedConsumer();
        var snapshots = new List<Task>();
        foreach (var channel in Channels)
        {
            var updates = consumer._updates.GetOrAdd(channel, _ => new ConcurrentQueue<JsonElement>());
            var events = consumer._systemEvents.GetOrAdd(channel, _ => new ConcurrentQueue<JsonElement>());
            var snapshot = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var connection = new HubConnectionBuilder().WithUrl(new Uri(host.Server.BaseAddress, $"/{channel}?ApiKey={apiKey}"), options =>
            {
                options.HttpMessageHandlerFactory = _ => host.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            }).Build();
            connection.On<JsonElement>(HubMethods.OnUpdate, items =>
            {
                updates.Enqueue(items);
                snapshot.TrySetResult();
            });
            connection.On<JsonElement>(HubMethods.OnSystemEvent, items => events.Enqueue(items));
            connection.On<long>(HubMethods.OnHeartbeat, sentAt =>
            {
                if (channel == ChannelNames.Trading)
                {
                    consumer._heartbeats.Enqueue(sentAt);
                }
            });
            consumer._connections.Add(channel, connection);
            await connection.StartAsync();
            snapshots.Add(snapshot.Task);
        }

        await Task.WhenAll(snapshots).WaitAsync(RunsClient.Patience);
        return consumer;
    }

    /// <summary>The non-empty <c>OnUpdate</c> payloads of a hub, in arrival order.</summary>
    public IReadOnlyList<JsonElement> Updates(string channel) =>
        _updates[channel].Where(items => items.GetArrayLength() > 0).ToArray();

    /// <summary>Every <c>OnUpdate</c> payload of a hub, the empty snapshot included.</summary>
    public IReadOnlyList<JsonElement> AllUpdates(string channel) => _updates[channel].ToArray();

    public IReadOnlyList<JsonElement> SystemEvents(string channel) => _systemEvents[channel].ToArray();

    public string ConnectionId(string channel) => _connections[channel].ConnectionId;

    public HubConnectionState State(string channel) => _connections[channel].State;

    /// <summary>Waits until a hub has delivered a number of non-empty updates.</summary>
    public async Task WaitForUpdatesAsync(string channel, int count, TimeSpan patience)
    {
        var watch = Stopwatch.StartNew();
        while (Updates(channel).Count < count)
        {
            Assert.True(watch.Elapsed < patience, $"{channel} delivered {Updates(channel).Count} of {count} updates within {patience}.");
            await Task.Delay(TimeSpan.FromMilliseconds(20));
        }
    }

    /// <summary>Waits until the trading hub has delivered a number of heartbeats.</summary>
    public async Task WaitForHeartbeatsAsync(int count, TimeSpan patience)
    {
        var watch = Stopwatch.StartNew();
        while (_heartbeats.Count < count)
        {
            Assert.True(watch.Elapsed < patience, $"The trading hub delivered {_heartbeats.Count} of {count} heartbeats within {patience}.");
            await Task.Delay(TimeSpan.FromMilliseconds(20));
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var connection in _connections.Values)
        {
            await connection.DisposeAsync();
        }
    }
}
