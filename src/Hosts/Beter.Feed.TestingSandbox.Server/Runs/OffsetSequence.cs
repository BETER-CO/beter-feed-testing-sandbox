#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>The one offset counter of the container, shared by every run. The first number is 1.</summary>
internal sealed class OffsetSequence
{
    private long _last;

    /// <summary>Takes the next number.</summary>
    /// <returns>The number.</returns>
    public long Next() => Interlocked.Increment(ref _last);
}
