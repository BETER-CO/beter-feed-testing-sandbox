namespace Beter.TestingTools.Emulator.Messaging.Options;

public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    public TopicsOptions Topics { get; set; }
    public ConsumerOptions ConsumerConfig { get; set; }
}