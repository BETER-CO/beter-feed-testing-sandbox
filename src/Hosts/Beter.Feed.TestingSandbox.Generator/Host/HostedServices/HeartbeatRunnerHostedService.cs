using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.Heartbeats;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Heartbeats;
using Beter.Feed.TestingSandbox.Generator.Host.Options;
using Beter.Feed.TestingSandbox.Generator.Application.Extensions;
using Beter.Feed.TestingSandbox.Models;

namespace Beter.Feed.TestingSandbox.Generator.Host.HostedServices;

public class HeartbeatRunnerHostedService : BackgroundService
{
    private readonly static TimeSpan _defaultTimerInterval = TimeSpan.FromSeconds(5);

    private readonly TimeSpan _timerInterval;
    private readonly IPublisher _publisher;
    private readonly ILogger<HeartbeatRunnerHostedService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public HeartbeatRunnerHostedService(IOptions<HeartbeatOptions> heartbeatOptions, IPublisher publisher, ILogger<HeartbeatRunnerHostedService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _timerInterval = TimeSpan.FromSeconds(heartbeatOptions?.Value?.IntervalInSeconds ?? _defaultTimerInterval.TotalSeconds);
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? NullLogger<HeartbeatRunnerHostedService>.Instance;
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[HeartbeatRunner] job started: {0}", DateTimeOffset.UtcNow);

        var timer = new PeriodicTimer(_timerInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessHeartbeatAsync(stoppingToken);
        }

        return;
    }

    private async Task ProcessHeartbeatAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var heartBeatService = scope.ServiceProvider.GetRequiredService<IHeartbeatControlService>();
        var currentCommand = heartBeatService.GetCurrentCommand();

        _logger.LogInformation("[HeartbeatRunner] Heartbeat State: {@currentCommand}", currentCommand.Name);

        if (HeartbeatCommand.IsRunStatus(currentCommand))
        {
            await SendHeartbeatAsync(cancellationToken);
        }

        return;
    }

    private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[HeartbeatRunner] Sending 'Heartbeat'");

        var heartbeat = new HeartbeatModel { HeartbeatTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };

        await _publisher
            .PublishAsync(heartbeat, cancellationToken)
            .ExecuteWithTimeout(
                _timerInterval,
                () => _logger.LogError("Timeout {Timeout} seconds while sending 'HeartBeat'", _timerInterval.Seconds));
    }
}