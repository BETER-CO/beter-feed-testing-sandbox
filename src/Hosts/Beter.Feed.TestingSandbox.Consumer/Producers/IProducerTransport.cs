using Confluent.Kafka;

namespace Beter.Feed.TestingSandbox.Consumer.Producers
{
    public interface IProducerTransport<T>
    {
        Task<DeliveryResult<string, byte[]>> ProduceAsync(string topic, T message, CancellationToken cancellationToken = default);
    }
}
