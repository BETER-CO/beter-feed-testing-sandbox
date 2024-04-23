using Confluent.Kafka;

namespace Beter.TestingTools.Emulator.Messaging;

public interface IGeneratorMessagesConsumer : IDisposable
{
    public Task StartConsuming(CancellationToken cancellationToken);
}
