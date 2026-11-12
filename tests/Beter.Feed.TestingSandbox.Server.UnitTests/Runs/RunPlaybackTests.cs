using System.Text.Json;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Server.Runs;
using Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Runs;

/// <summary>
/// Runs played to consumers on the four hubs of the host, with the cases of
/// <c>Fixtures/runs</c> loaded through <c>POST /api/cases</c>. The sportId of every item of
/// <c>four-channels</c> is the script index of its entry, so the order is visible.
/// </summary>
public class RunPlaybackTests : IClassFixture<SandboxHost>
{
    /// <summary>The sportId of each update of <c>four-channels</c>, per hub, in <c>at</c> order.</summary>
    private static readonly Dictionary<string, int[][]> FourChannels = new(StringComparer.Ordinal)
    {
        [ChannelNames.Timetable] = [[0], [7]],
        [ChannelNames.Trading] = [[1], [6, 6]],
        [ChannelNames.Scoreboard] = [[2]],
        [ChannelNames.Incident] = [[4]],
    };

    private readonly SandboxHost _host;

    public RunPlaybackTests(SandboxHost host)
    {
        _host = host;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    public async Task Run_SendsEveryMessageEntryToTheConsumerOfItsKey_PerChannelInAtOrder_AndNothingToAnotherKey(double acceleration)
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "four-channels");
        var apiKey = Guid.NewGuid();
        await using var consumer = await FeedConsumer.ConnectAsync(_host, apiKey);
        await using var other = await FeedConsumer.ConnectAsync(_host, Guid.NewGuid());

        var run = await RunsClient.StartAsync(_host, new { caseId, apiKey = apiKey.ToString(), acceleration });
        await RunsClient.WaitForStatusAsync(_host, run.GetProperty("runId").GetString(), "completed", RunsClient.Patience);
        foreach (var (channel, updates) in FourChannels)
        {
            await consumer.WaitForUpdatesAsync(channel, updates.Length, RunsClient.Patience);
        }

        foreach (var (channel, updates) in FourChannels)
        {
            Assert.Equal(updates, consumer.Updates(channel).Select(SportIds).ToArray());
        }

        Assert.Single(consumer.SystemEvents(ChannelNames.Incident));
        Assert.All(FeedConsumer.Channels, channel => Assert.Empty(other.Updates(channel)));
        Assert.All(FeedConsumer.Channels, channel => Assert.Empty(other.SystemEvents(channel)));
        Assert.All(FeedConsumer.Channels, channel => Assert.NotEmpty(other.AllUpdates(channel)));
    }

    [Fact]
    public async Task Run_GivesTheRecordedMatchOneNewGuid_TheSameOnTheFourChannels()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "four-channels");
        var apiKey = Guid.NewGuid();
        await using var consumer = await FeedConsumer.ConnectAsync(_host, apiKey);

        var run = await RunsClient.StartAsync(_host, new { caseId, apiKey = apiKey.ToString(), acceleration = 20 });
        await RunsClient.WaitForStatusAsync(_host, run.GetProperty("runId").GetString(), "completed", RunsClient.Patience);
        await consumer.WaitForUpdatesAsync(ChannelNames.Timetable, 2, RunsClient.Patience);

        var ids = MatchIds(consumer).Concat(consumer.SystemEvents(ChannelNames.Incident).SelectMany(Ids)).Distinct().ToArray();
        var id = Assert.Single(ids);
        Assert.True(Guid.TryParse(id, out var guid) && guid.ToString("D") == id, $"{id} is not a GUID in the feed's text form.");
    }

    [Fact]
    public async Task TwoRunsUnderOneKey_PlayAtOnce_AsTwoMatchesEachConsistentAcrossTheChannels()
    {
        var first = await RunsClient.LoadCaseAsync(_host, "four-channels");
        var second = await RunsClient.LoadCaseAsync(_host, "second-match");
        var apiKey = Guid.NewGuid();
        await using var consumer = await FeedConsumer.ConnectAsync(_host, apiKey);

        var runs = await Task.WhenAll(
            RunsClient.StartAsync(_host, new { caseId = first, apiKey = apiKey.ToString() }),
            RunsClient.StartAsync(_host, new { caseId = second, apiKey = apiKey.ToString() }));
        var finished = await Task.WhenAll(runs.Select(run =>
            RunsClient.WaitForStatusAsync(_host, run.GetProperty("runId").GetString(), "completed", RunsClient.Patience)));
        await consumer.WaitForUpdatesAsync(ChannelNames.Trading, 3, RunsClient.Patience);
        await consumer.WaitForUpdatesAsync(ChannelNames.Timetable, 3, RunsClient.Patience);
        await consumer.WaitForUpdatesAsync(ChannelNames.Scoreboard, 2, RunsClient.Patience);
        await consumer.WaitForUpdatesAsync(ChannelNames.Incident, 2, RunsClient.Patience);

        var ids = MatchIds(consumer).Distinct().ToArray();
        Assert.Equal(2, ids.Length);
        Assert.All(FeedConsumer.Channels, channel =>
            Assert.Equal(ids.Order(StringComparer.Ordinal), consumer.Updates(channel).SelectMany(Ids).Distinct().Order(StringComparer.Ordinal)));
        Assert.True(
            RunsClient.Instant(finished[0], "startedAt") < RunsClient.Instant(finished[1], "finishedAt")
                && RunsClient.Instant(finished[1], "startedAt") < RunsClient.Instant(finished[0], "finishedAt"),
            "The two runs did not play at the same time.");
    }

    [Fact]
    public async Task OnConnectRun_WithoutAConnection_EndsFailedAfterTheWaitTimeout()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "four-channels");

        var run = await RunsClient.StartAsync(_host, new { caseId, apiKey = Guid.NewGuid().ToString(), waitTimeoutSeconds = 1 });
        var failed = await RunsClient.WaitForStatusAsync(_host, run.GetProperty("runId").GetString(), "failed", RunsClient.Patience);

        Assert.Equal("waiting", run.GetProperty("status").GetString());
        Assert.Equal("no connection joined", failed.GetProperty("error").GetString());
        Assert.Equal(JsonValueKind.Null, failed.GetProperty("startedAt").ValueKind);
        Assert.True(RunsClient.Instant(failed, "finishedAt") - RunsClient.Instant(failed, "createdAt") >= TimeSpan.FromMilliseconds(950));
    }

    [Fact]
    public async Task NowRun_WithoutAConnection_PlaysToTheEnd()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "four-channels");

        var run = await RunsClient.StartAsync(_host, new { caseId, apiKey = Guid.NewGuid().ToString(), start = "now", acceleration = 20 });
        var completed = await RunsClient.WaitForStatusAsync(_host, run.GetProperty("runId").GetString(), "completed", RunsClient.Patience);

        Assert.Equal("playing", run.GetProperty("status").GetString());
        Assert.Equal(run.GetProperty("createdAt").GetString(), completed.GetProperty("startedAt").GetString());
        Assert.Equal(JsonValueKind.Null, completed.GetProperty("error").ValueKind);
    }

    [Fact]
    public async Task OpenConnections_ReceiveASecondRunUnderTheSameKey_AfterTheFirstCompletes()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "second-match");
        var apiKey = Guid.NewGuid();
        await using var consumer = await FeedConsumer.ConnectAsync(_host, apiKey);
        var connectionIds = FeedConsumer.Channels.Select(consumer.ConnectionId).ToArray();

        var first = await RunsClient.StartAsync(_host, new { caseId, apiKey = apiKey.ToString(), acceleration = 20 });
        await RunsClient.WaitForStatusAsync(_host, first.GetProperty("runId").GetString(), "completed", RunsClient.Patience);
        await consumer.WaitForUpdatesAsync(ChannelNames.Incident, 1, RunsClient.Patience);
        var second = await RunsClient.StartAsync(_host, new { caseId, apiKey = apiKey.ToString(), acceleration = 20 });
        await RunsClient.WaitForStatusAsync(_host, second.GetProperty("runId").GetString(), "completed", RunsClient.Patience);

        Assert.Equal("playing", second.GetProperty("status").GetString());
        foreach (var channel in FeedConsumer.Channels)
        {
            await consumer.WaitForUpdatesAsync(channel, 2, RunsClient.Patience);
        }

        Assert.Equal(connectionIds, FeedConsumer.Channels.Select(consumer.ConnectionId));
        Assert.All(FeedConsumer.Channels, channel => Assert.Equal(HubConnectionState.Connected, consumer.State(channel)));
        Assert.Equal(2, MatchIds(consumer).Distinct().Count());
    }

    [Fact]
    public async Task Acceleration_ScalesTheScriptClockOnly_HeartbeatsKeepTheirRealInterval()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "forty-seconds");
        var apiKey = Guid.NewGuid();
        await using var consumer = await FeedConsumer.ConnectAsync(_host, apiKey);

        var run = await RunsClient.StartAsync(_host, new { caseId, apiKey = apiKey.ToString(), acceleration = 20 });
        var completed = await RunsClient.WaitForStatusAsync(_host, run.GetProperty("runId").GetString(), "completed", RunsClient.Patience);
        await consumer.WaitForUpdatesAsync(ChannelNames.Trading, 3, RunsClient.Patience);

        var played = RunsClient.Instant(completed, "finishedAt") - RunsClient.Instant(completed, "startedAt");
        Assert.InRange(played, TimeSpan.FromMilliseconds(1950), TimeSpan.FromSeconds(10));
        await consumer.WaitForHeartbeatsAsync(2, RunsClient.Patience);
        var heartbeats = consumer.TradingHeartbeats.Order().ToArray();
        Assert.All(heartbeats.Zip(heartbeats.Skip(1), (earlier, later) => later - earlier), gap => Assert.True(gap >= 500, $"Heartbeats {gap} ms apart."));
    }

    [Fact]
    public async Task WaitTimeout_IsRealTime_NotScaledByTheAcceleration()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "forty-seconds");

        var run = await RunsClient.StartAsync(_host, new { caseId, apiKey = Guid.NewGuid().ToString(), acceleration = 20, waitTimeoutSeconds = 1 });
        var failed = await RunsClient.WaitForStatusAsync(_host, run.GetProperty("runId").GetString(), "failed", RunsClient.Patience);

        Assert.True(RunsClient.Instant(failed, "finishedAt") - RunsClient.Instant(failed, "createdAt") >= TimeSpan.FromMilliseconds(950));
    }

    [Fact]
    public async Task Run_OverMessagePack_SendsTheRecordedValuesOfObjectTypedFields()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "four-channels");
        var apiKey = Guid.NewGuid();
        var update = new TaskCompletionSource<object[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_host.Server.BaseAddress, $"/trading?ApiKey={apiKey}"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _host.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .AddMessagePackProtocol(options =>
                options.SerializerOptions = MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance))
            .Build();
        connection.On<object[]>(HubMethods.OnUpdate, items =>
        {
            if (items.Length > 0)
            {
                update.TrySetResult(items);
            }
        });
        await connection.StartAsync();

        await RunsClient.StartAsync(_host, new { caseId, apiKey = apiKey.ToString(), acceleration = 20 });
        var item = (IDictionary<object, object>)(await update.Task.WaitAsync(RunsClient.Patience))[0];

        var market = (IDictionary<object, object>)((object[])item["markets"])[0];
        var prices = (IDictionary<object, object>)((IDictionary<object, object>)((object[])market["outcomes"])[0])["prices"];
        Assert.Equal(1.53, Convert.ToDouble(prices["decimal"], System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal("-188", prices["american"]);
    }

    [Fact]
    public async Task OnConnectRun_CreatedBeforeAnyConnection_StartsWhenAConnectionPresentsItsKey()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "second-match");
        var apiKey = Guid.NewGuid();
        var run = await RunsClient.StartAsync(_host, new { caseId, apiKey = apiKey.ToString(), acceleration = 20 });
        var runId = run.GetProperty("runId").GetString();
        var waiting = await RunsClient.GetAsync(_host, runId);

        await using var consumer = await FeedConsumer.ConnectAsync(_host, apiKey);
        var completed = await RunsClient.WaitForStatusAsync(_host, runId, "completed", RunsClient.Patience);

        Assert.Equal("waiting", waiting.GetProperty("status").GetString());
        Assert.True(RunsClient.Instant(completed, "startedAt") >= RunsClient.Instant(completed, "createdAt"));
        await consumer.WaitForUpdatesAsync(ChannelNames.Incident, 1, RunsClient.Patience);
    }

    [Fact]
    public async Task OnConnectRun_AfterTheConsumerOfItsKeyDisconnected_WaitsAgainAndFailsAfterItsTimeout()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "second-match");
        var apiKey = Guid.NewGuid();
        var registry = _host.Services.GetRequiredService<RunRegistry>();
        var consumer = await FeedConsumer.ConnectAsync(_host, apiKey);
        await WaitUntilAsync(() => registry.OpenConnections(apiKey) == FeedConsumer.Channels.Length);

        await consumer.DisposeAsync();
        await WaitUntilAsync(() => registry.OpenConnections(apiKey) == 0);
        var run = await RunsClient.StartAsync(_host, new { caseId, apiKey = apiKey.ToString(), waitTimeoutSeconds = 1 });
        var failed = await RunsClient.WaitForStatusAsync(_host, run.GetProperty("runId").GetString(), "failed", RunsClient.Patience);

        Assert.Equal("waiting", run.GetProperty("status").GetString());
        Assert.Equal("no connection joined", failed.GetProperty("error").GetString());
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task ConnectionRefusedForItsKey_IsCountedForNoKey(string apiKey)
    {
        var host = new SandboxHost();
        await host.InitializeAsync();
        try
        {
            var registry = host.Services.GetRequiredService<RunRegistry>();
            var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            await using var connection = new HubConnectionBuilder()
                .WithUrl(new Uri(host.Server.BaseAddress, $"/trading?ApiKey={apiKey}"), options =>
                {
                    options.HttpMessageHandlerFactory = _ => host.Server.CreateHandler();
                    options.Transports = HttpTransportType.LongPolling;
                })
                .Build();
            connection.Closed += _ =>
            {
                closed.TrySetResult();
                return Task.CompletedTask;
            };

            await connection.StartAsync();
            await closed.Task.WaitAsync(RunsClient.Patience);

            Assert.Equal(0, registry.ConnectedKeys);
            Assert.Equal(0, registry.OpenConnections(Guid.Empty));
        }
        finally
        {
            await host.DisposeAsync();
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (!condition())
        {
            Assert.True(watch.Elapsed < RunsClient.Patience, "The registry did not count the connections in time.");
            await Task.Delay(TimeSpan.FromMilliseconds(20));
        }
    }

    private static int[] SportIds(JsonElement items) =>
        items.EnumerateArray().Select(item => item.GetProperty("sportId").GetInt32()).ToArray();

    private static IEnumerable<string> Ids(JsonElement items) =>
        items.EnumerateArray().Select(item => item.GetProperty("id").GetString());

    private static IEnumerable<string> MatchIds(FeedConsumer consumer) =>
        FeedConsumer.Channels.SelectMany(channel => consumer.Updates(channel).SelectMany(Ids));
}
