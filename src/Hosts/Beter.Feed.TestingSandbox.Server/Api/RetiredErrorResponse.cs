#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Api;

/// <summary>The body of a <c>410</c>, <c>RetiredError</c> of the description: an error naming the case that replaces a retired one.</summary>
/// <param name="Error">A short code of the error.</param>
/// <param name="Detail">What went wrong.</param>
/// <param name="ReplacedBy">The id of the replacing case; null where none replaces it.</param>
internal sealed record RetiredErrorResponse(string Error, string Detail, int? ReplacedBy);
