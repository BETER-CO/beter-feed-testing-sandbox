namespace Beter.Feed.TestingSandbox.Consumer.Models;

public sealed class FeedMessageModel<TValue>
{
    public string Channel { get; }
    public string Method { get; }
    public TValue Value { get; }

    public FeedMessageModel(TValue value, string channel, string method)
    {
        Method = method ?? throw new ArgumentNullException(nameof(method));
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }
}
