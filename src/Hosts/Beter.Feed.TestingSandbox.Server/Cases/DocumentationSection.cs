#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Cases;

/// <summary>One <c>covers</c> entry of a case file.</summary>
/// <param name="Page">A page of the public feed documentation.</param>
/// <param name="Section">A section on that page.</param>
public sealed record DocumentationSection(string Page, string Section);
