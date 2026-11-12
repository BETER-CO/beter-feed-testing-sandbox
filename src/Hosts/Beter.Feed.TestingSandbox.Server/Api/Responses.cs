#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Api;

/// <summary>The answer of <c>GET /api/version</c>.</summary>
/// <param name="SandboxVersion">The same string as the git tag, the image tag and <c>info.version</c>.</param>
/// <param name="OutputVersion">The version of the run output schemas the sandbox writes.</param>
/// <param name="Contract">The newest contract pin among the promoted cases; null while no promoted case carries one.</param>
public sealed record VersionResponse(string SandboxVersion, int OutputVersion, object? Contract);

/// <summary>The answer of <c>GET /api/health</c>.</summary>
/// <param name="Status">Always <c>ok</c>.</param>
public sealed record HealthResponse(string Status);

/// <summary>The body of an error response.</summary>
/// <param name="Error">A short code of the error.</param>
/// <param name="Detail">What went wrong.</param>
public sealed record ErrorResponse(string Error, string Detail);

/// <summary>The body of a <c>422</c>: an error with the failed keywords of the case schema.</summary>
/// <param name="Error">A short code of the error.</param>
/// <param name="Detail">What went wrong.</param>
/// <param name="Errors">The JSON Schema errors, one per failed keyword.</param>
public sealed record ValidationErrorResponse(string Error, string Detail, IReadOnlyList<Cases.SchemaError> Errors);
