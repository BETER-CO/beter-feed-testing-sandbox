using Beter.Feed.TestingSandbox.IntegrationTests.Options;
using Confluent.Kafka;

namespace Beter.Feed.TestingSandbox.IntegrationTests.Infrastructure.Kafka
{
    public sealed class KafkaConsumer : IDisposable
    {
        private IConsumer<byte[], byte[]> _consumer;
        private readonly ConsumerConfig _consumerConfig;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private bool _disposed;
        private Task _consumerTask;

        public KafkaConsumer(ConsumerOptions consumerOptions)
        {
            _consumerConfig = InitConsumerConfig(consumerOptions);
            _consumer = new ConsumerBuilder<byte[], byte[]>(_consumerConfig).Build();
        }

        private static ConsumerConfig InitConsumerConfig(ConsumerOptions consumerSettings)
        {
            var config = new ConsumerConfig
            {
                GroupId = consumerSettings.GroupId,
                BootstrapServers = consumerSettings.BootstrapServers,
                EnableAutoCommit = consumerSettings.EnableAutoCommit ?? true,
                PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range,
                AllowAutoCreateTopics = consumerSettings.AllowAutoCreateTopics,
                AutoOffsetReset = consumerSettings.AutoOffsetReset ?? AutoOffsetReset.Earliest
            };

            return config;
        }

        public Task StartConsuming(string topicName, Action<ConsumeResult<byte[], byte[]>> handler)
        {
            _consumerTask = Task.Factory.StartNew(
                () => StartConsumerLoop(topicName, handler, _cancellationTokenSource.Token),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            return _consumerTask;
        }

        private void StartConsumerLoop(string topicName, Action<ConsumeResult<byte[], byte[]>> handler, CancellationToken cancellationToken)
        {
            _consumer.Subscribe(topicName);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(cancellationToken);
                        if (consumeResult == null || consumeResult.IsPartitionEOF)
                        {
                            continue;
                        }

                        handler(consumeResult);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }
            finally
            {
                _consumer?.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _cancellationTokenSource.Cancel();
                _consumerTask?.Wait();

                _consumer?.Dispose();
                _cancellationTokenSource.Dispose();
            }

            _disposed = true;
        }
    }
}
