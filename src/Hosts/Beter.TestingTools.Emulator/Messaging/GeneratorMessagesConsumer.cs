using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Emulator.Messaging.Handlers.Abstract;
using Beter.TestingTools.Emulator.Messaging.Options;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace Beter.TestingTools.Emulator.Messaging;

public class GeneratorMessagesConsumer : IGeneratorMessagesConsumer
{
    private IConsumer<byte[], byte[]> _consumer;
    private readonly ConsumerConfig _consumerConfig;
    private readonly ILogger<GeneratorMessagesConsumer> _logger;
    private readonly IMessageHandlerResolver _messageHandlerResolver;
    private readonly IConsumeMessageConverter _messageConverter;
    private readonly ConcurrentDictionary<string, ActionBlock<ConsumeMessageContext>> _actionBlocks = new();

    private string _topic;
    private bool disposedValue;

    public GeneratorMessagesConsumer(
        IMessageHandlerResolver messageHandlerResolver,
        IConsumeMessageConverter messageConverter,
        IOptions<MessagingOptions> messagingOptions,
        ILogger<GeneratorMessagesConsumer> logger)
    {
        var settings = messagingOptions?.Value ?? throw new ArgumentNullException(nameof(messagingOptions));

        _consumerConfig = InitConsumerConfig(settings.ConsumerConfig);
        _consumer = InitConsumer();
        _topic = settings.Topics.TestingTools;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageConverter = messageConverter ?? throw new ArgumentNullException(nameof(messageConverter));
        _messageHandlerResolver = messageHandlerResolver ?? throw new ArgumentNullException(nameof(messageHandlerResolver));
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

        if (!string.IsNullOrWhiteSpace(consumerSettings.SslCertificateLocation) &&
            !string.IsNullOrWhiteSpace(consumerSettings.SslKeyLocation))
        {
            config.SslCertificateLocation = consumerSettings.SslCertificateLocation;
            config.SslKeyLocation = consumerSettings.SslKeyLocation;
            config.SecurityProtocol = SecurityProtocol.Ssl;
        }

        return config;
    }

    private IConsumer<byte[], byte[]> InitConsumer()
    {
        var consumer = new ConsumerBuilder<byte[], byte[]>(_consumerConfig)
            .SetLogHandler((_, msg) => _logger.LogInformation("{facility}; {name}; {message}", msg.Facility, msg.Name, msg.Message))
            .SetErrorHandler((sender, error) => _logger.LogError("Error occured while consuming messages {@error}.", error))
            .SetStatisticsHandler((sender, stat) => _logger.LogDebug("Kafka statistics: {stat}.", stat))
            .Build();

        return consumer;
    }

    public Task StartConsuming(CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(
            _ => StartConsumerLoop(cancellationToken), TaskCreationOptions.LongRunning);
    }

    private async Task StartConsumerLoop(CancellationToken cancellationToken)
    {
        _consumer?.Subscribe(_topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer?.Consume(cancellationToken);

                if (consumeResult == null || consumeResult.IsPartitionEOF || !_messageConverter.CanProcess(consumeResult))
                {
                    continue;
                }

                var context = _messageConverter.ConvertToMessageContextFromConsumeResult(consumeResult);
                if (!context.MessageHeaders.TryGetValue(HeaderNames.PlaybackId, out var playbackId))
                {
                    throw new ArgumentException($"Unsupported message from {_topic} without playback.");
                }

                var actionBlock = _actionBlocks.GetOrAdd(playbackId, CreateActionBlock);

                var result = await actionBlock.SendAsync(context, cancellationToken);
                if (!result)
                {
                    _logger.LogWarning($"Action block processing is not acknowledge sending mesage to internal queue.");
                }

                _logger.LogInformation("Message with type '{messageType}' was successfully processed.", context.MessageType);

            }
            catch (ConsumeException consumeException)
            {
                _logger.LogError(consumeException, "Error occurred while consuming messages.");
            }
            catch (OperationCanceledException canceledException)
            {
                _logger.LogWarning(canceledException, "Consuming canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred.");
            }
        }
    }

    private ActionBlock<ConsumeMessageContext> CreateActionBlock(string playbackId)
    {
        return new ActionBlock<ConsumeMessageContext>(
            async context =>
            {
                try
                {
                    var handler = _messageHandlerResolver.Resolve(context);
                    await handler.HandleAsync(context, CancellationToken.None);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error occurred while processing message '{message}'.", e.Message);
                }
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1,
                BoundedCapacity = 100,
                EnsureOrdered = true
            });
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _consumer?.Close();
                _consumer?.Dispose();

                foreach (var actionBlock in _actionBlocks.ToArray())
                {
                    actionBlock.Value.Complete();
                }

                var timeout = TimeSpan.FromSeconds(3);
                if (!Task.WaitAll(_actionBlocks.ToArray().Select(pair => pair.Value.Completion).ToArray(), timeout))
                {
                    _logger.LogError($"Cannot wait for {nameof(_actionBlocks)}");
                }
            }

            disposedValue = true;
        }
    }
}
