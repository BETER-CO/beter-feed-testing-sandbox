using Confluent.Kafka;

namespace Beter.TestingTools.Emulator.Messaging.Options;

public sealed class ConsumerOptions
{
    public string BootstrapServers { get; set; }
    public int? ConnectionTimeout { get; set; }
    public int? AutoCommitInterval { get; set; }
    public string GroupId { get; set; }
    public bool? EnableAutoCommit { get; set; }
    public string SslKeyLocation { get; set; }
    public string SslCertificateLocation { get; set; }
    public AutoOffsetReset? AutoOffsetReset { get; set; }
    public bool AllowAutoCreateTopics { get; set; } = true;
    public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.Plaintext;
}

