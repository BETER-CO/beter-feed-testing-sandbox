using System.Collections;
using System.Collections.Concurrent;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Sinks;

namespace Beter.Feed.TestingSandbox.EndToEndTests;

/// <summary>
/// Keeps the first message the sample consumer received per hub and hub method, and the item
/// count of every non-empty <c>OnUpdate</c> per hub.
/// </summary>
public sealed class RecordingSink : IFeedMessageSink
{
    private readonly ConcurrentDictionary<(string Channel, string Method), TaskCompletionSource<object>> _first = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<int>> _updates = new(StringComparer.Ordinal);

    /// <summary>Counts the items of a collection payload.</summary>
    /// <param name="payload">The payload.</param>
    /// <returns>The number of items.</returns>
    public static int CountOf(object payload) => ((IEnumerable)payload).Cast<object>().Count();

    public Task HandleAsync(string channel, string method, object payload)
    {
        Entry(channel, method).TrySetResult(payload);
        if (method == HubMethods.OnUpdate && CountOf(payload) is var count and > 0)
        {
            _updates.GetOrAdd(channel, _ => new ConcurrentQueue<int>()).Enqueue(count);
        }

        return Task.CompletedTask;
    }

    /// <summary>The item counts of the non-empty updates of a hub, in arrival order.</summary>
    /// <param name="channel">The hub.</param>
    /// <returns>The counts.</returns>
    public IReadOnlyList<int> UpdateCounts(string channel) =>
        _updates.TryGetValue(channel, out var counts) ? counts.ToArray() : [];

    /// <summary>Waits for the first message of a hub method on a hub.</summary>
    /// <param name="channel">The hub.</param>
    /// <param name="method">The hub method.</param>
    /// <param name="patience">How long to wait.</param>
    /// <returns>The payload of the message.</returns>
    public Task<object> First(string channel, string method, TimeSpan patience) =>
        Entry(channel, method).Task.WaitAsync(patience);

    private TaskCompletionSource<object> Entry(string channel, string method) => _first.GetOrAdd(
        (channel, method),
        _ => new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously));
}
