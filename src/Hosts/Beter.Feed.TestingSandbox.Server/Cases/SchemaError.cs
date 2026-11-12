#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Cases;

/// <summary>One failed keyword of the case schema.</summary>
/// <param name="InstanceLocation">A JSON Pointer into the case.</param>
/// <param name="KeywordLocation">A JSON Pointer into the case schema.</param>
/// <param name="Error">What failed.</param>
public sealed record SchemaError(string InstanceLocation, string KeywordLocation, string Error);
