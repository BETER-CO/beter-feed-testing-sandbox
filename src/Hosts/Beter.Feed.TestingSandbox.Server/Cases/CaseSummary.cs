#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Cases;

/// <summary>The fields of a case file the catalogue lists.</summary>
/// <param name="Version">The case's own version.</param>
/// <param name="Class">The class slug.</param>
/// <param name="Sport">The <c>sportId</c> value; null for a case that belongs to no sport.</param>
/// <param name="Tags">The tags of the case.</param>
/// <param name="Description">The description.</param>
/// <param name="Terminal">The state the case ends in.</param>
/// <param name="Covers">The <c>covers</c> entries of the case file.</param>
/// <param name="Duration">The feed time the script spans, in seconds.</param>
public sealed record CaseSummary(
    string Version,
    string Class,
    long? Sport,
    IReadOnlyList<string> Tags,
    string Description,
    string Terminal,
    IReadOnlyList<DocumentationSection> Covers,
    long Duration);
