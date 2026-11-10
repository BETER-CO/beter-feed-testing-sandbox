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
    private readonly CancellationTokenSource _stoppingCts = new();
    private Task _connectTask = Task.CompletedTask;

    public ConsumerBase(T feedServiceClient, ILogger logger)
    {
        _feedServiceClient = feedServiceClient ?? throw new ArgumentNullException(nameof(logger));
        _logger = logger ?? NullLogger<ConsumerBase<T>>.Instance;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{typeof(T).Name} is running.");

        // Connect in the background so a missing server does not block host start-up;
        // the client retries until it connects or the host stops.
        _connectTask = _feedServiceClient.ConnectAsync(_stoppingCts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{typeof(T).Name} is stopping.");

        _stoppingCts.Cancel();
        await _connectTask.WaitAsync(cancellationToken);
        await _feedServiceClient.DisconnectAsync(cancellationToken);
    }
}
