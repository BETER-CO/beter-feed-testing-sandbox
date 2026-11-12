using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Beter.Feed.TestingSandbox.Server.Cases;
using Beter.Feed.TestingSandbox.Server.Runs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Runs;

/// <summary>The player on a fake clock: due times, stop, failures and directives.</summary>
public sealed class RunPlayerTests : IDisposable
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 12, 10, 0, 0, TimeSpan.Zero));
    private readonly RecordingSender _sender;
    private readonly RunPlayer _player;
    private readonly List<Run> _runs = [];

    public RunPlayerTests()
    {
        _sender = new RecordingSender(_time);
        _player = new RunPlayer(_time, _sender, new OffsetSequence(), NullLogger.Instance);
    }

    public void Dispose()
    {
        foreach (var run in _runs)
        {
            run.Dispose();
        }
    }

    [Fact]
    public async Task Player_SendsEachEntryAtStartedAtPlusAtOverTheAcceleration_InAtOrder_StableForEqualAt()
    {
        var run = NewRun(acceleration: 2, start: RunStart.Now);
        var playback = _player.PlayAsync(run, Script(
            Trading(0, 3000), Trading(1, 1000), Trading(2, 1000), Trading(3, 0)));

        await _sender.WaitForCountAsync(1);
        _time.Advance(TimeSpan.FromMilliseconds(499));
        await AssertStaysAsync(1);
        _time.Advance(TimeSpan.FromMilliseconds(1));
        await _sender.WaitForCountAsync(3);
        _time.Advance(TimeSpan.FromMilliseconds(1000));
        await playback.WaitAsync(Patience);

        int[] order = [3, 1, 2, 0];
        Assert.Equal(order, _sender.Sent.Select(sent => SportId(sent.message)));
        Assert.Equal(new long[] { 0, 500, 500, 1500 }, _sender.Sent.Select(sent => (long)(sent.at - run.CreatedAt).TotalMilliseconds));
        Assert.Equal(RunStatus.Completed, run.Status);
        Assert.Equal(run.CreatedAt + TimeSpan.FromMilliseconds(1500), Instant(run, "finishedAt"));
    }

    [Fact]
    public async Task Player_StoppedMidRun_SendsNothingMore()
    {
        var run = NewRun(acceleration: 1, start: RunStart.Now);
        var playback = _player.PlayAsync(run, Script(Trading(0, 0), Trading(1, 1000), Trading(2, 2000)));
        await _sender.WaitForCountAsync(1);

        Assert.True(run.TryStop(_time.GetUtcNow()));
        await playback.WaitAsync(Patience);
        _time.Advance(TimeSpan.FromSeconds(5));

        Assert.Single(_sender.Sent);
        Assert.Equal(RunStatus.Stopped, run.Status);
        Assert.False(run.TryComplete(_time.GetUtcNow()));
    }

    [Fact]
    public async Task Player_FailsTheRunOnAnUnsupportedMessageType_NamingTheScriptIndex()
    {
        var run = NewRun(acceleration: 1, start: RunStart.Now);

        await _player.PlayAsync(run, Script(Trading(0, 0), """{ "at": 0, "channel": "trading", "messageType": "odds", "value": [] }""", Trading(2, 0)))
            .WaitAsync(Patience);

        Assert.Equal(RunStatus.Failed, run.Status);
        Assert.Equal("script entry 1: unsupported messageType 'odds'", run.ToManifest()["error"]!.GetValue<string>());
        Assert.Single(_sender.Sent);
    }

    [Fact]
    public async Task Player_FailsTheRunOnACaseFileItCannotRead()
    {
        var run = NewRun(acceleration: 1, start: RunStart.Now);

        await _player.PlayAsync(run, _ => throw new InvalidDataException("The case file of 1 is not JSON.")).WaitAsync(Patience);

        Assert.Equal(RunStatus.Failed, run.Status);
        Assert.Equal("The case file of 1 is not JSON.", run.ToManifest()["error"]!.GetValue<string>());
    }

    [Fact]
    public async Task Player_PassesOverADirective_WithNoEffect()
    {
        var run = NewRun(acceleration: 1, start: RunStart.Now);
        var playback = _player.PlayAsync(run, Script(
            Trading(0, 0), """{ "at": 500, "directive": "connectionDrop", "channels": ["trading"] }""", Trading(2, 1000)));
        await _sender.WaitForCountAsync(1);

        _time.Advance(TimeSpan.FromMilliseconds(1000));
        await playback.WaitAsync(Patience);

        int[] sent = [0, 2];
        Assert.Equal(sent, _sender.Sent.Select(sent => SportId(sent.message)));
        Assert.Equal(RunStatus.Completed, run.Status);
    }

    [Fact]
    public async Task Player_OfAnOnConnectRun_FailsWhenNoConnectionJoinsWithinTheWaitTimeout()
    {
        var run = NewRun(acceleration: 20, start: RunStart.OnConnect, waitTimeout: TimeSpan.FromSeconds(5));
        var playback = _player.PlayAsync(run, Script(Trading(0, 0)));

        _time.Advance(TimeSpan.FromMilliseconds(4999));
        await AssertStaysAsync(0);
        Assert.Equal(RunStatus.Waiting, run.Status);
        _time.Advance(TimeSpan.FromMilliseconds(1));
        await playback.WaitAsync(Patience);

        Assert.Equal(RunStatus.Failed, run.Status);
        Assert.Equal(RunPlayer.NoConnectionJoined, run.ToManifest()["error"]!.GetValue<string>());
        Assert.Empty(_sender.Sent);
    }

    [Fact]
    public async Task Player_OfAnOnConnectRun_StartsTheClockWhenAConnectionJoins()
    {
        var run = NewRun(acceleration: 1, start: RunStart.OnConnect, waitTimeout: TimeSpan.FromSeconds(5));
        var playback = _player.PlayAsync(run, Script(Trading(0, 0), Trading(1, 1000)));

        _time.Advance(TimeSpan.FromSeconds(2));
        Assert.True(run.TryStart(_time.GetUtcNow()));
        await _sender.WaitForCountAsync(1);
        _time.Advance(TimeSpan.FromSeconds(1));
        await playback.WaitAsync(Patience);

        Assert.Equal(RunStatus.Completed, run.Status);
        Assert.Equal(new long[] { 2000, 3000 }, _sender.Sent.Select(sent => (long)(sent.at - run.CreatedAt).TotalMilliseconds));
    }

    [Fact]
    public async Task Player_OfAnOnConnectRun_WaitsAWaitTimeoutBeyondTheLongestSingleDelay()
    {
        var run = NewRun(acceleration: 1, start: RunStart.OnConnect, waitTimeout: TimeSpan.FromSeconds(int.MaxValue));
        var playback = _player.PlayAsync(run, Script(Trading(0, 0)));

        _time.Advance(TimeSpan.FromDays(60));
        await AssertStaysAsync(0);
        Assert.Equal(RunStatus.Waiting, run.Status);
        Assert.True(run.TryStart(_time.GetUtcNow()));
        await playback.WaitAsync(Patience);

        Assert.Equal(RunStatus.Completed, run.Status);
        Assert.Single(_sender.Sent);
    }

    [Fact]
    public async Task Player_SendsAnEntryDueBeyondTheLongestSingleDelay_WhenItIsDue()
    {
        var run = NewRun(acceleration: 0.25, start: RunStart.Now);
        var playback = _player.PlayAsync(run, Script(Trading(0, 0), Trading(1, 1_500_000_000)));
        await _sender.WaitForCountAsync(1);

        _time.Advance(TimeSpan.FromDays(60));
        await AssertStaysAsync(1);
        Assert.Equal(RunStatus.Playing, run.Status);
        _time.Advance(TimeSpan.FromMilliseconds(6_000_000_000) - TimeSpan.FromDays(60));
        await playback.WaitAsync(Patience);

        Assert.Equal(RunStatus.Completed, run.Status);
        Assert.Equal(6_000_000_000, (long)(_sender.Sent[1].at - run.CreatedAt).TotalMilliseconds);
    }

    private static string Trading(int sportId, long at) =>
        $$"""{ "at": {{at}}, "channel": "trading", "messageType": "trading", "value": [ { "id": "m1", "sportId": {{sportId}}, "timestamp": 1720000000000, "msgType": 1 } ] }""";

    private static Func<CancellationToken, Task<CaseScript>> Script(params string[] entries) =>
        _ => Task.FromResult(CaseScript.Parse(JsonDocument.Parse($"{{ \"script\": [ {string.Join(", ", entries)} ] }}")));

    private static int SportId(PlaybackMessage message) =>
        ((Models.TradingInfos.TradingInfoModel[])message.Payload)[0].SportId!.Value;

    private static DateTimeOffset Instant(Run run, string field) =>
        DateTimeOffset.Parse(run.ToManifest()[field]!.GetValue<string>(), System.Globalization.CultureInfo.InvariantCulture);

    private Run NewRun(double acceleration, string start, TimeSpan? waitTimeout = null)
    {
        var playedCase = new CaseEntry
        {
            CaseId = 1,
            Summary = new CaseSummary("1.0.0", "baseline", null, [], "Player test.", "Finished", [], 3),
            Content = Encoding.UTF8.GetBytes("{}"),
        };
        var now = _time.GetUtcNow();
        var run = new Run("r-20261112-1000-0001", Guid.NewGuid(), playedCase, acceleration, start, waitTimeout ?? TimeSpan.FromSeconds(60), "3.0.0", now);
        _runs.Add(run);
        if (start == RunStart.Now)
        {
            run.TryStart(now);
        }

        return run;
    }

    /// <summary>
    /// Gives a playback that has nothing due the chance to send anyway, then checks it did
    /// not. The fake clock does not move, so no due time can pass while it waits.
    /// </summary>
    private async Task AssertStaysAsync(int count)
    {
        for (var i = 0; i < 20; i++)
        {
            await Task.Yield();
            await Task.Delay(TimeSpan.FromMilliseconds(5));
        }

        Assert.Equal(count, _sender.Sent.Length);
    }

    private sealed class RecordingSender : IPlaybackSender
    {
        private readonly TimeProvider _time;
        private readonly ConcurrentQueue<(DateTimeOffset at, PlaybackMessage message)> _sent = new();

        public RecordingSender(TimeProvider time)
        {
            _time = time;
        }

        public (DateTimeOffset at, PlaybackMessage message)[] Sent => _sent.ToArray();

        public Task SendAsync(Guid apiKey, PlaybackMessage message, CancellationToken cancellationToken)
        {
            _sent.Enqueue((_time.GetUtcNow(), message));
            return Task.CompletedTask;
        }

        public async Task WaitForCountAsync(int count)
        {
            var watch = Stopwatch.StartNew();
            while (_sent.Count < count)
            {
                Assert.True(watch.Elapsed < Patience, $"{_sent.Count} of {count} entries were sent.");
                await Task.Delay(TimeSpan.FromMilliseconds(5));
            }
        }
    }
}
