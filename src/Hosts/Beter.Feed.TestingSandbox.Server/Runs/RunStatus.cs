#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>The status words of a run, as <c>run.schema.json</c> names them.</summary>
internal static class RunStatus
{
    /// <summary>The script clock has not started.</summary>
    public const string Waiting = "waiting";

    /// <summary>The script clock runs.</summary>
    public const string Playing = "playing";

    /// <summary>The last script entry was sent.</summary>
    public const string Completed = "completed";

    /// <summary>The run was stopped, by a client or by the host shutting down.</summary>
    public const string Stopped = "stopped";

    /// <summary>The sandbox could not play the case to its end; <c>error</c> says why.</summary>
    public const string Failed = "failed";

    /// <summary>Gets every status word, in lifecycle order.</summary>
    public static IReadOnlyList<string> All { get; } = [Waiting, Playing, Completed, Stopped, Failed];

    /// <summary>Whether a status word is a known one.</summary>
    /// <param name="status">The status word.</param>
    /// <returns>True for a word of <see cref="All"/>.</returns>
    public static bool IsKnown(string? status) => status is not null && All.Contains(status, StringComparer.Ordinal);

    /// <summary>Whether a status is one a run never leaves.</summary>
    /// <param name="status">The status word.</param>
    /// <returns>True for <c>completed</c>, <c>stopped</c> and <c>failed</c>.</returns>
    public static bool IsTerminal(string status) => status is Completed or Stopped or Failed;
}
