#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using Beter.Feed.TestingSandbox.Server.Cases;

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>
/// Every run the container holds, oldest first, and the open hub connections per key. A
/// connection presenting a key starts the clock of the <c>onConnect</c> runs waiting under
/// it. The host shutting down stops every live run.
/// </summary>
internal sealed class RunRegistry : IHostedService, IDisposable
{
    private readonly object _gate = new();
    private readonly List<Run> _runs = [];
    private readonly HashSet<string> _issued = new(StringComparer.Ordinal);
    private readonly Dictionary<Guid, int> _connections = [];
    private readonly TimeProvider _time;
    private readonly RunPlayer _player;
    private readonly string _sandboxVersion;
    private bool _shuttingDown;

    /// <summary>Initializes a new instance of the <see cref="RunRegistry"/> class.</summary>
    /// <param name="time">The clock of the timestamps.</param>
    /// <param name="player">Plays the runs.</param>
    /// <param name="sandboxVersion">The sandbox version the manifests name.</param>
    public RunRegistry(TimeProvider time, RunPlayer player, string sandboxVersion)
    {
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _sandboxVersion = sandboxVersion;
    }

    /// <summary>Gets the number of keys with at least one open hub connection.</summary>
    public int ConnectedKeys
    {
        get
        {
            lock (_gate)
            {
                return _connections.Count;
            }
        }
    }

    /// <summary>Creates a run and starts its playback.</summary>
    /// <param name="playedCase">The case, with its case file.</param>
    /// <param name="apiKey">The key that binds the run.</param>
    /// <param name="acceleration">The acceleration.</param>
    /// <param name="start">When the script clock starts, <see cref="RunStart"/>.</param>
    /// <param name="waitTimeout">How long an <c>onConnect</c> run waits for a first connection.</param>
    /// <returns>The run.</returns>
    public Run Create(CaseEntry playedCase, Guid apiKey, double acceleration, string start, TimeSpan waitTimeout)
    {
        Run run;
        lock (_gate)
        {
            var now = _time.GetUtcNow();
            run = new Run(NewRunId(now), apiKey, playedCase, acceleration, start, waitTimeout, _sandboxVersion, now);
            _runs.Add(run);
            if (_shuttingDown)
            {
                // The host is stopping: the run is created stopped and never plays.
                run.TryStop(now);
                return run;
            }

            if (start == RunStart.Now || _connections.ContainsKey(apiKey))
            {
                run.TryStart(now);
            }

            run.Playback = Task.Run(() => _player.PlayAsync(run, token => CaseScript.LoadAsync(playedCase, token)));
        }

        return run;
    }

    /// <summary>Lists the runs, oldest first.</summary>
    /// <param name="status">The status a run must have; null for every run.</param>
    /// <returns>The runs.</returns>
    public IReadOnlyList<Run> List(string? status)
    {
        lock (_gate)
        {
            return _runs.Where(run => status is null || run.Status == status).ToArray();
        }
    }

    /// <summary>Finds a run.</summary>
    /// <param name="runId">The run id.</param>
    /// <returns>The run, or null for an unknown id.</returns>
    public Run? Find(string runId)
    {
        lock (_gate)
        {
            return _runs.Find(run => string.Equals(run.RunId, runId, StringComparison.Ordinal));
        }
    }

    /// <summary>Stops a live run.</summary>
    /// <param name="runId">The run id.</param>
    /// <param name="run">The run; null for an unknown id.</param>
    /// <returns><see cref="RunOutcome.Conflict"/> for a run in a terminal status.</returns>
    public RunOutcome Stop(string runId, out Run? run)
    {
        run = Find(runId);
        if (run is null)
        {
            return RunOutcome.NotFound;
        }

        return run.TryStop(_time.GetUtcNow()) ? RunOutcome.Done : RunOutcome.Conflict;
    }

    /// <summary>Removes a run in a terminal status; its id then answers as unknown.</summary>
    /// <param name="runId">The run id.</param>
    /// <returns><see cref="RunOutcome.Conflict"/> for a live run.</returns>
    public RunOutcome Delete(string runId)
    {
        Run? run;
        lock (_gate)
        {
            run = _runs.Find(candidate => string.Equals(candidate.RunId, runId, StringComparison.Ordinal));
            if (run is null)
            {
                return RunOutcome.NotFound;
            }

            if (run.IsLive)
            {
                return RunOutcome.Conflict;
            }

            _runs.Remove(run);
        }

        run.Dispose();
        return RunOutcome.Done;
    }

    /// <summary>A hub connection presenting a key is open: the waiting <c>onConnect</c> runs under it start.</summary>
    /// <param name="apiKey">The key.</param>
    public void ConnectionJoined(Guid apiKey)
    {
        lock (_gate)
        {
            _connections[apiKey] = _connections.GetValueOrDefault(apiKey) + 1;
            var now = _time.GetUtcNow();
            foreach (var run in _runs.Where(run => run.ApiKey == apiKey && run.Status == RunStatus.Waiting))
            {
                run.TryStart(now);
            }
        }
    }

    /// <summary>A hub connection presenting a key is closed.</summary>
    /// <param name="apiKey">The key.</param>
    public void ConnectionLeft(Guid apiKey)
    {
        lock (_gate)
        {
            var open = _connections.GetValueOrDefault(apiKey) - 1;
            if (open > 0)
            {
                _connections[apiKey] = open;
            }
            else
            {
                _connections.Remove(apiKey);
            }
        }
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>The number of open hub connections presenting a key.</summary>
    /// <param name="apiKey">The key.</param>
    /// <returns>The count.</returns>
    public int OpenConnections(Guid apiKey)
    {
        lock (_gate)
        {
            return _connections.GetValueOrDefault(apiKey);
        }
    }

    /// <summary>
    /// Ends every <c>waiting</c> or <c>playing</c> run as <c>stopped</c>; a run created
    /// afterwards is created stopped.
    /// </summary>
    /// <param name="cancellationToken">Ends the wait for the playbacks to halt.</param>
    /// <returns>The stop.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Run[] live;
        lock (_gate)
        {
            _shuttingDown = true;
            live = _runs.Where(run => run.IsLive).ToArray();
        }

        var now = _time.GetUtcNow();
        foreach (var run in live)
        {
            StopForShutdown(run, now);
        }

        try
        {
            await Task.WhenAll(live.Select(run => run.Playback)).WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The host does not wait longer; the runs are stopped already.
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_gate)
        {
            foreach (var run in _runs)
            {
                run.Dispose();
            }

            _runs.Clear();
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "One run that cannot be stopped must not keep the others playing.")]
    private static void StopForShutdown(Run run, DateTimeOffset now)
    {
        try
        {
            run.TryStop(now);
        }
        catch (Exception)
        {
            // The run is terminal or disposed already; the next one is stopped regardless.
        }
    }

    /// <summary>A new id, <c>r-yyyyMMdd-HHmm-xxxx</c> in UTC, never issued before in the container.</summary>
    private string NewRunId(DateTimeOffset now)
    {
        string runId;
        do
        {
            runId = string.Create(
                CultureInfo.InvariantCulture,
                $"r-{now.UtcDateTime:yyyyMMdd}-{now.UtcDateTime:HHmm}-{RandomNumberGenerator.GetInt32(0x10000):x4}");
        }
        while (!_issued.Add(runId));

        return runId;
    }
}
