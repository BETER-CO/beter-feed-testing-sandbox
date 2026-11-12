namespace Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;

/// <summary>The host before its case set is loaded.</summary>
public sealed class UnloadedSandboxHost : SandboxHost
{
    protected override bool LoadsCaseSet => false;
}
