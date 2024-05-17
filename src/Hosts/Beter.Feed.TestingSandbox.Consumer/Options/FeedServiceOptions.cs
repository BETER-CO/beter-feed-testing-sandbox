namespace Beter.Feed.TestingSandbox.Consumer.Options;

public class FeedServiceOptions
{
    public static string SectionName => "FeedService";

    public required string ApiKey { get; set; }
    public required string Host { get; set; }
    public bool SkipNegotiation { get; set; }
    public long Offset { get; set; }
    public int SnapshotBatchSize { get; set; }
    public double ReconnectionWaitSeconds { get; set; } = 1;
    public string DestinationTopicName { get; set; } = "feed-messages-test";
}