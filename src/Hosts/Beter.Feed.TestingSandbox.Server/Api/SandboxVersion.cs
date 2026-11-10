#nullable enable

using System.Reflection;

namespace Beter.Feed.TestingSandbox.Server.Api;

/// <summary>
/// The sandbox version: one string shared by the git tag, the image tag, <c>info.version</c>
/// of the served description and <c>GET /api/version</c>. A tag build passes the tag as
/// <c>-p:Version</c>; any other build reports the version of <c>Directory.Build.props</c>.
/// </summary>
/// <param name="Value">The version string, without build metadata.</param>
public sealed record SandboxVersion(string Value)
{
    /// <summary>Gets the <c>major.minor.patch</c> part, without a pre-release suffix.</summary>
    public string Core => Value.Split('-', 2)[0];

    /// <summary>Reads the version this assembly was built with.</summary>
    /// <returns>The version of this build.</returns>
    public static SandboxVersion OfThisBuild() => Of(typeof(SandboxVersion).Assembly);

    /// <summary>Reads the version an assembly was built with.</summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns>The informational version, without build metadata.</returns>
    public static SandboxVersion Of(Assembly assembly)
    {
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? throw new InvalidOperationException($"{assembly.GetName().Name} carries no informational version.");
        return new SandboxVersion(informational.Split('+', 2)[0]);
    }
}
