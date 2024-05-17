using Confluent.Kafka;

namespace Beter.Feed.TestingSandbox.Consumer.Producers
{
    public interface IProducerFactory
    {
        public IProducer<TKey, TValue> Create<TKey, TValue>(PublishOptions config);
    }
}
