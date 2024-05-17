using Beter.Feed.TestingSandbox.Generator.Application.Contracts;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services;

public sealed class RunCountTracker : IRunCountTracker
{
    private int _runId = 0;

    public int GetNext() => Interlocked.Increment(ref _runId);
}