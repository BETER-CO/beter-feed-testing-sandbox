namespace Beter.Feed.TestingSandbox.Consumer.Sinks;

/// <summary>
/// Receives every message a feed hub client gets from the server.
/// </summary>
public interface IFeedMessageSink
{
    /// <summary>
    /// Handles one message received from a hub.
    /// </summary>
    /// <param name="channel">The hub the message came from, one of <c>ChannelNames</c>.</param>
    /// <param name="method">The hub method that delivered it, one of <c>HubMethods</c>.</param>
    /// <param name="payload">
    /// The received payload: an array of feed models for <c>OnUpdate</c>, an array of
    /// <c>GlobalMessageModel</c> for <c>OnSystemEvent</c>, a <c>long</c> for <c>OnHeartbeat</c>
    /// and an array of subscription ids for <c>OnSubscriptionsRemove</c>.
    /// </param>
    Task HandleAsync(string channel, string method, object payload);
}
