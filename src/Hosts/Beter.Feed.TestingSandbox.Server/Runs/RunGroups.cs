#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>The SignalR group of the connections that present one feed <c>ApiKey</c>.</summary>
internal static class RunGroups
{
    /// <summary>The group of a key.</summary>
    /// <param name="apiKey">The key, as a GUID.</param>
    /// <returns>The group name.</returns>
    public static string Of(Guid apiKey) => "api-key:" + apiKey.ToString("D");
}
