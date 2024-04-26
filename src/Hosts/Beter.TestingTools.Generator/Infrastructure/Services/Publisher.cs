using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Beter.TestingTools.Generator.Application.Contracts;
using Beter.TestingTools.Generator.Infrastructure.Options;
using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Infrastructure.Services;

public class Publisher : IPublisher
{
    private const string HeartbeatPlaybackId = "heartbeat-playback";

    private readonly PublishOptions _options;
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<Publisher> _logger;

    public Publisher(IOptions<PublishOptions> options, ILogger<Publisher> logger)
    {
        ValidateOptions(options.Value);
        _options = options.Value;
        _logger = logger;

        _producer = CreateProducer(options.Value);
    }

    private IProducer<string, string> CreateProducer(PublishOptions options)
    {
        var configDict = CreateProdcuerOptions(options);
        var config = new ProducerConfig(configDict);

        return new ProducerBuilder<string, string>(config)
            .SetErrorHandler(HandleError)
            .SetLogHandler(HandleLog)
            .Build();
    }

    private void HandleLog(IProducer<string, string> producer, LogMessage e)
    {
        _logger.LogDebug($"{e.Level}|{e.Name}|{e.Facility} - {e.Message}");
    }

    private void HandleError(IProducer<string, string> producer, Error e)
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

    public async Task PublishAsync(HeartbeatModel model, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Publish Heartbeat message to topic {GeneratorTopic} At: {Time}", _options.Topic, DateTimeOffset.UtcNow);

            await _producer.ProduceAsync(_options.Topic, CreateHeartbeatMessage(model), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish heartbeat to topic {GeneratorTopic}", _options.Topic);
        }
    }


    public async Task PublishEmptyAsync(string messageType, string channel, string playbackId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Publish message to topic {GeneratorTopic} At: {Time}", _options.Topic, DateTimeOffset.UtcNow);

            await _producer.ProduceAsync(_options.Topic, CreateFeedMessage(new List<IDictionary<string, object>>(), messageType, channel, playbackId), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to topic {GeneratorTopic}", _options.Topic);
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }

    private static Message<string, string> CreateFeedMessage(IEnumerable<IDictionary<string, object>> messages, string messageType, string channel, string playbackId)
    {
        return new Message<string, string>
        {
            Headers = new Headers
            {
                { HeaderNames.MessageType, Encoding.UTF8.GetBytes(messageType) },
                { HeaderNames.MessageChannel, Encoding.UTF8.GetBytes(channel) },
                { HeaderNames.PlaybackId, Encoding.UTF8.GetBytes(playbackId) }
            },
            Value = JsonSerializer.Serialize(messages)
        };
    }

    private static Message<string, string> CreateHeartbeatMessage(HeartbeatModel model)
    {
        var typeAsBytes = Encoding.UTF8.GetBytes(MessageTypes.Heartbeat);

        return new Message<string, string>
        {
            Headers = new Headers
            {
                { HeaderNames.MessageType, typeAsBytes },
                { HeaderNames.PlaybackId, Encoding.UTF8.GetBytes(HeartbeatPlaybackId) }
            },
            Value = JsonSerializer.Serialize(model)
        };
    }

    public async Task PublishAsync(TestScenarioMessage message, string playbackId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Publish message to topic {GeneratorTopic} At: {Time}", _options.Topic, DateTimeOffset.UtcNow);

            await _producer.ProduceAsync(_options.Topic, CreateFeedMessage(message, playbackId), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to topic {GeneratorTopic}", _options.Topic);
        }
    }

    private static Message<string, string> CreateFeedMessage(TestScenarioMessage message, string playbackId)
    {
        return new Message<string, string>
        {
            Headers = new Headers
            {
                { HeaderNames.MessageType, Encoding.UTF8.GetBytes(message.MessageType) },
                { HeaderNames.MessageChannel, Encoding.UTF8.GetBytes(message.Channel) },
                { HeaderNames.PlaybackId, Encoding.UTF8.GetBytes(playbackId) }
            },
            Value = message.Value.ToJsonString()
        };
    }
}