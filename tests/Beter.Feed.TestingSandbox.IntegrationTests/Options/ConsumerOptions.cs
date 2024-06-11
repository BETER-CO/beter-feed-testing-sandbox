using Confluent.Kafka;

namespace Beter.Feed.TestingSandbox.IntegrationTests.Options
{
    public sealed class ConsumerOptions
    {
        public const string Section = "ConsumerConfig";

        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }
        public bool? EnableAutoCommit { get; set; }
        public AutoOffsetReset? AutoOffsetReset { get; set; }
        public bool AllowAutoCreateTopics { get; set; } = true;
    }
}
