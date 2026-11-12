#nullable enable

using System.Globalization;
using System.Text.Json.Nodes;
using Beter.Feed.TestingSandbox.Server.Api;
using Beter.Feed.TestingSandbox.Server.Cases;

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>
/// One run of one case: what it was started with, its status and its manifest. The status
/// moves <c>waiting</c>, <c>playing</c>, then one of <c>completed</c>, <c>stopped</c> and
/// <c>failed</c>, and never leaves a terminal status.
/// </summary>
internal sealed class Run : IDisposable
{
    private const string TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

    private readonly object _gate = new();
    private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CancellationTokenSource _stopping = new();
    private readonly CaseSummary _summary;
    private string _status = RunStatus.Waiting;
    private string? _error;
    private DateTimeOffset? _startedAt;
    private DateTimeOffset? _finishedAt;
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="Run"/> class, <c>waiting</c>.</summary>
    /// <param name="runId">The run id.</param>
    /// <param name="apiKey">The key that binds the run.</param>
    /// <param name="playedCase">The case the run plays.</param>
    /// <param name="acceleration">The acceleration of the script clock.</param>
    /// <param name="start">When the script clock starts, <see cref="RunStart"/>.</param>
    /// <param name="waitTimeout">How long an <c>onConnect</c> run waits for a first connection.</param>
    /// <param name="sandboxVersion">The sandbox version the manifest names.</param>
    /// <param name="createdAt">The creation instant.</param>
    public Run(
        string runId,
        Guid apiKey,
        CaseEntry playedCase,
        double acceleration,
        string start,
        TimeSpan waitTimeout,
        string sandboxVersion,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(playedCase);
        RunId = runId;
        ApiKey = apiKey;
        Case = playedCase;
        _summary = playedCase.Summary ?? throw new ArgumentException("A run plays a case that has a case file.", nameof(playedCase));
        Acceleration = acceleration;
        Start = start;
        WaitTimeout = waitTimeout;
        SandboxVersion = sandboxVersion;
        CreatedAt = createdAt;
    }

    /// <summary>Gets the run id.</summary>
    public string RunId { get; }

    /// <summary>Gets the key that binds the run.</summary>
    public Guid ApiKey { get; }

    /// <summary>Gets the case the run plays.</summary>
    public CaseEntry Case { get; }

    /// <summary>Gets the acceleration of the script clock.</summary>
    public double Acceleration { get; }

    /// <summary>Gets when the script clock starts, <see cref="RunStart"/>.</summary>
    public string Start { get; }

    /// <summary>Gets how long an <c>onConnect</c> run waits for a first connection, in real time.</summary>
    public TimeSpan WaitTimeout { get; }

    /// <summary>Gets the sandbox version the manifest names.</summary>
    public string SandboxVersion { get; }

    /// <summary>Gets the creation instant.</summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>Gets or sets the playback of the run; it never fails.</summary>
    public Task Playback { get; set; } = Task.CompletedTask;

    /// <summary>Gets the task that completes when the script clock starts.</summary>
    public Task Started => _started.Task;

    /// <summary>Gets the token that is cancelled when the run is stopped.</summary>
    public CancellationToken Stopping => _stopping.Token;

    /// <summary>Gets the status word.</summary>
    public string Status
    {
        get
        {
            lock (_gate)
            {
                return _status;
            }
        }
    }

    /// <summary>Gets the instant the script clock started; null until it does.</summary>
    public DateTimeOffset? StartedAt
    {
        get
        {
            lock (_gate)
            {
                return _startedAt;
            }
        }
    }

    /// <summary>Gets a value indicating whether the run is <c>waiting</c> or <c>playing</c>.</summary>
    public bool IsLive => !RunStatus.IsTerminal(Status);

    /// <summary>Starts the script clock of a <c>waiting</c> run.</summary>
    /// <param name="at">The instant the clock starts at.</param>
    /// <returns>True when the run was waiting and now plays.</returns>
    public bool TryStart(DateTimeOffset at)
    {
        lock (_gate)
        {
            if (_status != RunStatus.Waiting)
            {
                return false;
            }

            _status = RunStatus.Playing;
            _startedAt = at;
        }

        _started.TrySetResult();
        return true;
    }

    /// <summary>Ends a <c>playing</c> run as <c>completed</c>.</summary>
    /// <param name="at">The instant the last entry was sent.</param>
    /// <returns>True when the run was playing.</returns>
    public bool TryComplete(DateTimeOffset at) => TryFinish(RunStatus.Completed, null, at, RunStatus.Playing);

    /// <summary>Ends a live run as <c>failed</c>.</summary>
    /// <param name="at">The instant of the failure.</param>
    /// <param name="error">What stopped the sandbox from playing.</param>
    /// <returns>True when the run was live.</returns>
    public bool TryFail(DateTimeOffset at, string error) => TryFinish(RunStatus.Failed, error, at, null);

    /// <summary>Ends a live run as <c>stopped</c> and halts its playback.</summary>
    /// <param name="at">The instant of the stop.</param>
    /// <returns>True when the run was live.</returns>
    public bool TryStop(DateTimeOffset at)
    {
        lock (_gate)
        {
            if (!TryFinish(RunStatus.Stopped, null, at, null))
            {
                return false;
            }

            // The token is cancelled before the gate opens, so no reader sees the run stopped
            // with its playback still allowed to send. The callbacks run off this thread.
            _ = _stopping.CancelAsync();
            return true;
        }
    }

    /// <summary>
    /// The manifest, <c>run.json</c> of <c>schemas/run-output/v1/run.schema.json</c>. Its
    /// <c>summary</c> is the empty block: the figures arrive with the run output.
    /// </summary>
    /// <returns>The manifest.</returns>
    public JsonObject ToManifest()
    {
        lock (_gate)
        {
            var playedCase = new JsonObject
            {
                ["caseId"] = Case.CaseId,
                ["version"] = _summary.Version,
                ["class"] = _summary.Class,
            };
            if (_summary.Sport is { } sport)
            {
                playedCase["sport"] = sport;
            }

            playedCase["tags"] = new JsonArray(_summary.Tags.Select(tag => (JsonNode?)tag).ToArray());
            playedCase["terminal"] = _summary.Terminal;

            return new JsonObject
            {
                ["outputVersion"] = SandboxApi.OutputVersion,
                ["runId"] = RunId,
                ["apiKey"] = ApiKey.ToString("D"),
                ["sandboxVersion"] = SandboxVersion,
                ["contract"] = _summary.ContractFetchedAt is { } fetchedAt
                    ? new JsonObject { ["fetchedAt"] = fetchedAt, ["newestChange"] = _summary.ContractNewestChange }
                    : null,
                ["case"] = playedCase,
                ["acceleration"] = Acceleration,
                ["start"] = Start,
                ["status"] = _status,
                ["error"] = _error,
                ["createdAt"] = Timestamp(CreatedAt),
                ["startedAt"] = _startedAt is { } startedAt ? Timestamp(startedAt) : null,
                ["finishedAt"] = _finishedAt is { } finishedAt ? Timestamp(finishedAt) : null,
                ["summary"] = EmptySummary(),
                ["expected"] = new JsonArray(_summary.Expected.Select(id => (JsonNode?)id).ToArray()),
            };
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_gate)
        {
            if (!_disposed)
            {
                _disposed = true;
                _stopping.Dispose();
            }
        }
    }

    private static string Timestamp(DateTimeOffset instant) =>
        instant.UtcDateTime.ToString(TimestampFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// The <c>summary</c> block with no figures. The run output (the transcript and the
    /// timeline) is what fills it; until it is built, every run reports this block.
    /// </summary>
    private static JsonObject EmptySummary() => new()
    {
        ["messages"] = new JsonObject(),
        ["offsets"] = new JsonObject(),
        ["matches"] = new JsonArray(),
        ["directives"] = new JsonArray(),
        ["connections"] = 0,
        ["snapshots"] = new JsonObject { ["connection"] = 0, ["recovery"] = 0, ["booking"] = 0 },
    };

    private bool TryFinish(string status, string? error, DateTimeOffset at, string? from)
    {
        lock (_gate)
        {
            if (RunStatus.IsTerminal(_status) || (from is not null && _status != from))
            {
                return false;
            }

            _status = status;
            _error = error;
            _finishedAt = at;
            return true;
        }
    }
}
