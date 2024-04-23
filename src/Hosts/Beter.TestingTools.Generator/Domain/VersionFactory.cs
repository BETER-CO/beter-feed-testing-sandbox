namespace Beter.TestingTool.Generator.Domain;

public static class VersionFactory
{
    public static Version CreateVersion(string versionString) => Version.TryParse(versionString, out var version) ? version : null;
}

