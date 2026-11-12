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
    long Duration)
{
    /// <summary>The acceleration a case accepts at most when it names none.</summary>
    public const double DefaultMaxAcceleration = 20;

    /// <summary>Gets the <c>fetchedAt</c> of the contract pin of the case; null for a case that carries none.</summary>
    public string? ContractFetchedAt { get; init; }

    /// <summary>Gets the <c>newestChange</c> of the contract pin of the case; null for a case that carries none.</summary>
    public string? ContractNewestChange { get; init; }

    /// <summary>Gets the highest acceleration a run of the case accepts.</summary>
    public double MaxAcceleration { get; init; } = DefaultMaxAcceleration;

    /// <summary>Gets the <c>start</c> of the case; null for a case that names none.</summary>
    public string? Start { get; init; }

    /// <summary>Gets the <c>id</c> of every <c>expected</c> entry of the case, in file order.</summary>
    public IReadOnlyList<string> Expected { get; init; } = [];
}
