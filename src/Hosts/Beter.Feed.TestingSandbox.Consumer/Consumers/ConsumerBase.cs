using Beter.Feed.TestingSandbox.Consumer.Clients;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Beter.Feed.TestingSandbox.Consumer.Consumers;

internal abstract class ConsumerBase<T> : IHostedService
    where T : IFeedClient
{
    private readonly ILogger _logger;
    private readonly T _feedServiceClient;

    public ConsumerBase(T feedServiceClient, ILogger logger)
    {
        _feedServiceClient = feedServiceClient ?? throw new ArgumentNullException(nameof(logger));
        _logger = logger ?? NullLogger<ConsumerBase<T>>.Instance;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{typeof(T).Name} is running.");
        await _feedServiceClient.ConnectAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(T)} is stopping.");
        await _feedServiceClient.DisconnectAsync(cancellationToken);
    }
}
