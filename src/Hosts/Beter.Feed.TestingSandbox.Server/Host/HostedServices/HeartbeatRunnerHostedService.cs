using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Extensions;
using Beter.Feed.TestingSandbox.Server.Host.Options;
using Beter.Feed.TestingSandbox.Server.Publishers;
using Beter.Feed.TestingSandbox.Server.Services.Heartbeats;
using Beter.Feed.TestingSandbox.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Beter.Feed.TestingSandbox.Server.Host.HostedServices
{
    public class HeartbeatRunnerHostedService : BackgroundService
    {
        private readonly static TimeSpan _defaultTimerInterval = TimeSpan.FromSeconds(5);

        private readonly TimeSpan _timerInterval;
        private readonly IEnumerable<IMessagePublisher> _publishers;
        private readonly ILogger<HeartbeatRunnerHostedService> _logger;
        private readonly IHeartbeatControlService _heartbeatControlService;

        public HeartbeatRunnerHostedService(IOptions<HeartbeatOptions> heartbeatOptions, IEnumerable<IMessagePublisher> publishers, ILogger<HeartbeatRunnerHostedService> logger, IHeartbeatControlService heartbeatControlService)
        {
            var seconds = heartbeatOptions?.Value?.IntervalInSeconds ?? 0;
            _timerInterval = seconds > 0 ? TimeSpan.FromSeconds(seconds) : _defaultTimerInterval;
            _publishers = publishers ?? throw new ArgumentNullException(nameof(publishers));
            _logger = logger ?? NullLogger<HeartbeatRunnerHostedService>.Instance;
            _heartbeatControlService = heartbeatControlService ?? throw new ArgumentNullException(nameof(heartbeatControlService));
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
            var currentCommand = _heartbeatControlService.GetCurrentCommand();

            _logger.LogDebug("[HeartbeatRunner] Heartbeat State: {@currentCommand}", currentCommand.Name);

            if (HeartbeatCommand.IsRunStatus(currentCommand))
            {
                await SendHeartbeatAsync(cancellationToken);
            }

            return;
        }

        private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("[HeartbeatRunner] Sending 'Heartbeat'");

            var tasks = _publishers
               .AsParallel()
               .Select(s => s.SendGroupOnHeartbeatAsync(GroupNames.DefaultGroupName, cancellationToken))
               .Select(s =>  s.ExecuteWithTimeout(_timerInterval, () => _logger.LogError("Timeout {Timeout} seconds while sending 'HeartBeat'", _timerInterval.Seconds)))
               .ToArray();

            await Task.WhenAll(tasks);
        }
    }
}
