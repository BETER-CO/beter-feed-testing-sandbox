using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Beter.Feed.TestingSandbox.Consumer.Producers
{
    public class ProducerFactory : IProducerFactory
    {
        private readonly ILogger<ProducerFactory> _logger;

        public ProducerFactory(ILogger<ProducerFactory> logger)
        {
            _logger = logger ?? NullLogger<ProducerFactory>.Instance;
        }

        public IProducer<TKey, TValue> Create<TKey, TValue>(PublishOptions config)
        {
            ValidateOptions(config);

            return CreateProducer<TKey, TValue>(config);
        }

        private IProducer<TKey, TValue> CreateProducer<TKey, TValue>(PublishOptions options)
        {
            var configDict = CreateProdcuerOptions(options);
            var config = new ProducerConfig(configDict);

            return new ProducerBuilder<TKey, TValue>(config)
                .SetErrorHandler(HandleError)
                .SetLogHandler(HandleLog)
            .Build();
        }

        private void HandleLog<TKey, TValue>(IProducer<TKey, TValue> producer, LogMessage e)
        {
            _logger.LogDebug($"{e.Level}|{e.Name}|{e.Facility} - {e.Message}");
        }

        private void HandleError<TKey, TValue>(IProducer<TKey, TValue> producer, Error e)
        {
            _logger.LogError($"Producer error. Reason={e.Reason}, Code={e.Code}, IsBrokerError={e.IsBrokerError}, IsLocalError={e.IsLocalError}");
        }

        private static void ValidateOptions(PublishOptions options)
        {
            if (options.SecurityProtocol == SecurityProtocol.Ssl)
            {
                if (string.IsNullOrEmpty(options.SslKeyLocation))
                    throw new ArgumentException("Invalid Kafka configuration: SslKeyLocation should be specified.");

                if (string.IsNullOrEmpty(options.SslCertificateLocation))
                    throw new ArgumentException("Invalid Kafka configuration: SslCertificateLocation should be specified.");
            }

            if (string.IsNullOrEmpty(options.BootstrapServers))
                throw new ArgumentException("Invalid Kafka configuration: BootstrapServers should be specified.");
        }

        private static ProducerConfig CreateProdcuerOptions(PublishOptions options)
        {
            return new ProducerConfig
            {
                BootstrapServers = options.BootstrapServers,
                SecurityProtocol = options.SecurityProtocol,
                SslCertificateLocation = options.SslCertificateLocation,
                SslKeyLocation = options.SslKeyLocation,
                AllowAutoCreateTopics = options.AllowAutoCreateTopics
            };
        }
    }
}
