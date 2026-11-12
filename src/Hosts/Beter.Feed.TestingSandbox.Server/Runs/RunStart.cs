#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>When the script clock of a run starts.</summary>
internal static class RunStart
{
    /// <summary>When a connection presenting the run's key is open.</summary>
    public const string OnConnect = "onConnect";

    /// <summary>At the run's creation.</summary>
    public const string Now = "now";
}
