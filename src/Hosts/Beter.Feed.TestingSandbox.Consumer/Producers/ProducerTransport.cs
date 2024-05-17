using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beter.Feed.TestingSandbox.Consumer.Producers
{
    public class ProducerTransport<T> : IProducerTransport<T>
    {
        private readonly IProducerMessageConverter<T> _producerMessageConverter;
        private readonly IProducer<string, byte[]> _producer;

        public ProducerTransport(IOptions<PublishOptions> publishOptions, IProducerMessageConverter<T> producerMessageConverter, IProducerFactory producerFactory)
        {
            ArgumentNullException.ThrowIfNull(producerFactory);
            ArgumentNullException.ThrowIfNull(publishOptions?.Value);

            _producerMessageConverter = producerMessageConverter ?? throw new ArgumentNullException(nameof(producerMessageConverter));
            _producer = producerFactory.Create<string, byte[]>(publishOptions.Value);
        }

        public async Task<DeliveryResult<string, byte[]>> ProduceAsync(string topic, T message, CancellationToken cancellationToken = default)
        {
            var kafkaMessage = _producerMessageConverter.ConvertToKafkaMessage(message);
            return await _producer.ProduceAsync(topic, kafkaMessage!, cancellationToken);
        }
    }
}
