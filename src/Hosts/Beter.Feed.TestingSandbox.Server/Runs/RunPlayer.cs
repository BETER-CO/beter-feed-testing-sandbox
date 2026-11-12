#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>
/// Plays the script of a run on its clock: entry i is due at <c>startedAt + at / acceleration</c>.
/// Directive entries are passed over. A case that cannot be played ends the run
/// <c>failed</c>; the playback itself never throws.
/// </summary>
internal sealed partial class RunPlayer
{
    /// <summary>The error of an <c>onConnect</c> run no connection joined in time.</summary>
    public const string NoConnectionJoined = "no connection joined";

    /// <summary>The longest single delay; a longer wait is taken in slices of it.</summary>
    private static readonly TimeSpan Slice = TimeSpan.FromDays(1);

    private readonly TimeProvider _time;
    private readonly IPlaybackSender _sender;
    private readonly OffsetSequence _offsets;
    private readonly ILogger _logger;

    /// <summary>Initializes a new instance of the <see cref="RunPlayer"/> class.</summary>
    /// <param name="time">The clock of the delays and timestamps.</param>
    /// <param name="sender">Sends the rewritten entries.</param>
    /// <param name="offsets">The offset counter of the container.</param>
    /// <param name="logger">The logger.</param>
    public RunPlayer(TimeProvider time, IPlaybackSender sender, OffsetSequence offsets, ILogger logger)
    {
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _offsets = offsets ?? throw new ArgumentNullException(nameof(offsets));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Plays a run to its end: waits for the clock to start, reads the script, sends every
    /// entry when due and ends the run <c>completed</c>, or <c>failed</c> with the cause.
    /// </summary>
    /// <param name="run">The run.</param>
    /// <param name="load">Reads the run's script; it runs off the caller's thread.</param>
    /// <returns>The playback; it completes when the run is terminal and never fails.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Any defect of a case or of a send ends the run failed; the host keeps running.")]
    public async Task PlayAsync(Run run, Func<CancellationToken, Task<CaseScript>> load)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(load);
        var token = run.Stopping;
        var loading = Task.Run(() => load(token), CancellationToken.None);
        try
        {
            if (!await WaitForStartAsync(run, loading, token))
            {
                return;
            }

            var script = await loading;
            await PlayEntriesAsync(run, script, token);
            run.TryComplete(_time.GetUtcNow());
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Stopped: the run is already terminal.
        }
        catch (Exception exception)
        {
            if (run.TryFail(_time.GetUtcNow(), exception.Message))
            {
                LogFailed(_logger, run.RunId, exception);
            }
        }
        finally
        {
            // The script holds the parsed case file; it is released once the run is over, or
            // once a read still under way when the run ended is done.
            // A read that fails after the run ended has its fault observed here.
            _ = loading.ContinueWith(
                static loaded =>
                {
                    if (loaded.IsCompletedSuccessfully)
                    {
                        loaded.Result.Dispose();
                    }
                    else
                    {
                        _ = loaded.Exception;
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Run {RunId} failed.")]
    private static partial void LogFailed(ILogger logger, string runId, Exception exception);

    /// <summary>An instant some milliseconds after another, or the last instant there is.</summary>
    private static DateTimeOffset Later(DateTimeOffset instant, double milliseconds) =>
        milliseconds < (DateTimeOffset.MaxValue - instant).TotalMilliseconds
            ? instant + TimeSpan.FromMilliseconds(milliseconds)
            : DateTimeOffset.MaxValue;

    private async Task<bool> WaitForStartAsync(Run run, Task<CaseScript> loading, CancellationToken token)
    {
        if (run.Started.IsCompleted)
        {
            return true;
        }

        using var waiting = CancellationTokenSource.CreateLinkedTokenSource(token);
        var timeout = DelayUntilAsync(Later(run.CreatedAt, run.WaitTimeout.TotalMilliseconds), waiting.Token);
        var first = await Task.WhenAny(run.Started, timeout, loading);
        if (first == loading)
        {
            await loading;
            await Task.WhenAny(run.Started, timeout);
        }

        await waiting.CancelAsync();
        token.ThrowIfCancellationRequested();
        if (run.Started.IsCompleted)
        {
            return true;
        }

        return !run.TryFail(_time.GetUtcNow(), NoConnectionJoined) && run.Started.IsCompleted;
    }

    /// <summary>Waits until an instant of the clock, however far off, in slices no delay refuses.</summary>
    private async Task DelayUntilAsync(DateTimeOffset due, CancellationToken token)
    {
        for (var wait = due - _time.GetUtcNow(); wait > TimeSpan.Zero; wait = due - _time.GetUtcNow())
        {
            await Task.Delay(wait < Slice ? wait : Slice, _time, token);
        }
    }

    private async Task PlayEntriesAsync(Run run, CaseScript script, CancellationToken token)
    {
        var startedAt = run.StartedAt ?? throw new InvalidOperationException("The script clock has not started.");
        var rewriter = new PlaybackRewriter(startedAt, run.Acceleration, _offsets, script.Bases);
        foreach (var entry in script.Timeline)
        {
            await DelayUntilAsync(Later(startedAt, entry.At / run.Acceleration), token);

            token.ThrowIfCancellationRequested();
            if (entry.IsDirective)
            {
                continue;
            }

            try
            {
                await _sender.SendAsync(run.ApiKey, rewriter.Rewrite(entry), token);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                throw new InvalidDataException(
                    string.Create(CultureInfo.InvariantCulture, $"script entry {entry.Index}: {exception.Message}"),
                    exception);
            }
        }
    }
}
