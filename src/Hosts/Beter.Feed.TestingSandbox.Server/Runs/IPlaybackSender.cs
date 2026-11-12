#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>Sends a rewritten entry to every connection that presents a run's key.</summary>
internal interface IPlaybackSender
{
    /// <summary>Sends one entry.</summary>
    /// <param name="apiKey">The run's key.</param>
    /// <param name="message">The rewritten entry.</param>
    /// <param name="cancellationToken">Cancels the send.</param>
    /// <returns>The send.</returns>
    Task SendAsync(Guid apiKey, PlaybackMessage message, CancellationToken cancellationToken);
}
