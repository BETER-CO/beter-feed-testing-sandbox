#nullable enable

using System.Runtime.InteropServices;

namespace Beter.Feed.TestingSandbox.Server.Cases;

/// <summary>One id of the catalogue: a case, or a retired id.</summary>
public sealed class CaseEntry
{
    /// <summary>Gets the case id.</summary>
    public required int CaseId { get; init; }

    /// <summary>Gets a value indicating whether <c>retired.json</c> lists the id.</summary>
    public bool Retired { get; init; }

    /// <summary>Gets the id of the case that replaces a retired one; null when none does.</summary>
    public int? ReplacedBy { get; init; }

    /// <summary>Gets the catalogue fields of the case file; null for a retired id that has no case file.</summary>
    public CaseSummary? Summary { get; init; }

    /// <summary>Gets the path of the case file of a case of the image; null otherwise.</summary>
    public string? FilePath { get; init; }

    /// <summary>Gets the case file of a case loaded at runtime, held in memory only; null otherwise.</summary>
    public ReadOnlyMemory<byte>? Content { get; init; }

    /// <summary>Opens the case file for reading.</summary>
    /// <returns>The case file, or null for a retired id that has no case file.</returns>
    public Stream? OpenFile()
    {
        if (Content is { } content)
        {
            return MemoryMarshal.TryGetArray(content, out var segment)
                ? new MemoryStream(segment.Array!, segment.Offset, segment.Count, writable: false)
                : new MemoryStream(content.ToArray(), writable: false);
        }

        return FilePath is null
            ? null
            : new FileStream(
                FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
    }
}
