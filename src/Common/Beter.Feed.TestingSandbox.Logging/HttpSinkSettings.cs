namespace Beter.Feed.TestingSandbox.Logging;

public class HttpSinkSettings : SinkSettings
{
    public const long DefaultQueueLimitBytes = 10 * 1024 * 1024; // 10 MB

    public const long DefaultLogEventsInBatchLimit = 1000;

    public const int DefaultBatchSizeLimitBytes = 5 * 1024 * 1024; // 5 MB

    public string? RequestUri { get; set; }

    public long? QueueLimitBytes { get; set; } = DefaultQueueLimitBytes;

    public long? LogEventsInBatchLimit { get; set; } = DefaultLogEventsInBatchLimit;

    public int? BatchSizeLimitBytes { get; set; } = DefaultBatchSizeLimitBytes;
}
