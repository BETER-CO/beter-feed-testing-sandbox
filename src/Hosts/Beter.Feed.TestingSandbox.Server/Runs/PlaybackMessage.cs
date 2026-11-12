#nullable enable

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>One rewritten script entry, ready to send to the connections of a run.</summary>
/// <param name="Channel">The hub it is sent on.</param>
/// <param name="Method">The hub method: <c>OnUpdate</c>, <c>OnSystemEvent</c> or <c>OnSubscriptionsRemove</c>.</param>
/// <param name="Payload">The argument: an array of the channel's model, of system events, or of match ids.</param>
internal sealed record PlaybackMessage(string Channel, string Method, object Payload);
