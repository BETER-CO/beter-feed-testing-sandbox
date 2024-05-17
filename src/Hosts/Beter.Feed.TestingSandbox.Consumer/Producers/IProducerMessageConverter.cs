using Confluent.Kafka;

namespace Beter.Feed.TestingSandbox.Consumer.Producers
{
    public interface IProducerMessageConverter<in T>
    {
        Message<string, byte[]> ConvertToKafkaMessage(T message);
    }
}
