#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Cases;

/// <summary>Where the sandbox finds its case set.</summary>
public sealed class CaseOptions
{
    /// <summary>The configuration section of these options.</summary>
    public const string SectionName = "Cases";

    /// <summary>
    /// Gets or sets the directory of the case files and <c>retired.json</c>. A relative path is taken from
    /// the directory of the host's binaries.
    /// </summary>
    public string Directory { get; set; } = "cases";
}
