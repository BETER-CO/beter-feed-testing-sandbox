#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Cases;

/// <summary>The outcome of loading a case at runtime.</summary>
/// <param name="Entry">The loaded case; null when the case was refused.</param>
/// <param name="Errors">Why the case was refused; empty when it was loaded.</param>
public sealed record RuntimeCaseResult(CaseEntry? Entry, IReadOnlyList<SchemaError> Errors);
