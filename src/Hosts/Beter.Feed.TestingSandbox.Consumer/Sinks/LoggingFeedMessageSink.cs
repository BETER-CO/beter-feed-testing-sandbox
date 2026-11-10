using System.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Beter.Feed.TestingSandbox.Consumer.Sinks;

/// <summary>
/// Default sink: writes one structured log line per received message.
/// </summary>
public sealed class LoggingFeedMessageSink : IFeedMessageSink
{
    private readonly ILogger<LoggingFeedMessageSink> _logger;

    public LoggingFeedMessageSink(ILogger<LoggingFeedMessageSink> logger)
    {
        _logger = logger ?? NullLogger<LoggingFeedMessageSink>.Instance;
    }

    public Task HandleAsync(string channel, string method, object payload)
    {
        if (payload is ICollection items)
        {
            _logger.LogInformation(
                "Received {Method} on {Channel} with {ItemCount} items",
                method,
                channel,
                items.Count);
        }
        else
        {
            _logger.LogInformation("Received {Method} on {Channel}", method, channel);
            _logger.LogDebug("Received {Method} on {Channel}: {Payload}", method, channel, payload);
        }

        return Task.CompletedTask;
    }
}
