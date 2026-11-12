#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>The outcome of stopping or deleting a run.</summary>
internal enum RunOutcome
{
    /// <summary>No run has the id.</summary>
    NotFound,

    /// <summary>The run's status does not allow the operation.</summary>
    Conflict,

    /// <summary>The operation is done.</summary>
    Done,
}
