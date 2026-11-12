namespace Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;

/// <summary>
/// The host as it is the moment it starts listening: its loader runs, and nothing waits for
/// it. Without a directory it loads the case set of the repository, as the image does.
/// </summary>
public sealed class StartingSandboxHost : SandboxHost
{
    private readonly string _caseDirectory;

    public StartingSandboxHost(string caseDirectory = null)
    {
        _caseDirectory = caseDirectory;
    }

    protected override string CaseDirectory => _caseDirectory;

    protected override bool WaitsForCaseSet => false;
}
