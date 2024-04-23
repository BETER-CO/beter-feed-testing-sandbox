﻿using Beter.TestingTools.Common.Enums;
using Beter.TestingTools.Common.Extensions;

namespace Beter.TestingTools.Emulator.Publishers;

public class FeedMessagePublisherResolver : IFeedMessagePublisherResolver
{
    private readonly IReadOnlyDictionary<HubKind, IMessagePublisher> _publishers;

    public FeedMessagePublisherResolver(IEnumerable<IMessagePublisher> publishers)
    {
        _publishers = publishers.ToDictionary(
            publisher => publisher.Hub,
            publisher => publisher);
    }

    public IMessagePublisher Resolve(string channel)
    {
        if (string.IsNullOrEmpty(channel))
            throw new ArgumentException("Channel cannot be null or empty.");

        var hubKind = HubEnumHelper.ToHub(channel);

        if (!_publishers.TryGetValue(hubKind, out var publisher))
            throw new InvalidOperationException($"Unsupported publisher for hubKind: {hubKind}.");

        return publisher;
    }
}

