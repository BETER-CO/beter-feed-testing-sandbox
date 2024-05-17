using Confluent.Kafka;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging;

public interface IGeneratorMessagesConsumer : IDisposable
{
    public Task StartConsuming(CancellationToken cancellationToken);
}
