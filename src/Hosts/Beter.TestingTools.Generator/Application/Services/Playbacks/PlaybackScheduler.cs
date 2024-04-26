using Beter.TestingTools.Generator.Application.Contracts;
using Beter.TestingTools.Generator.Application.Contracts.Playbacks;
using Beter.TestingTools.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTools.Generator.Domain.Playbacks;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace Beter.TestingTools.Generator.Application.Services.Playbacks;

public class PlaybackScheduler : IPlaybackScheduler
{
    private readonly TimeSpan _idleWaitTime = TimeSpan.FromSeconds(10);

    private readonly ISystemClock _systemClock;
    private readonly IPlaybackRepository _playbacksRepository;
    private readonly ITestScenarioMessageHandlerResolver _messageHandlerResolver;
    private readonly ILogger<PlaybackScheduler> _logger;

    private readonly object _lock = new();
    private DateTimeOffset? _nextRunTime;
    private readonly ManualResetEventSlim _signalEvent = new(false);
    private readonly ConcurrentDictionary<string, ActionBlock<PlaybackItem>> _actionBlocks = new();

    public PlaybackScheduler(ITestScenarioMessageHandlerResolver messageHandlerResolver, IPlaybackRepository playbacksRepository, ISystemClock systemClock, ILogger<PlaybackScheduler> logger, IPublisher publisher)
    {
        _playbacksRepository = playbacksRepository ?? throw new ArgumentNullException(nameof(playbacksRepository));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _logger = logger ?? NullLogger<PlaybackScheduler>.Instance;
        _messageHandlerResolver = messageHandlerResolver ?? throw new ArgumentNullException(nameof(messageHandlerResolver));

        _playbacksRepository.PlaybackAdded += OnPlaybackAdded;
    }

    private void OnPlaybackAdded(object sender, PlaybackEventArgs e)
    {
        var newPlaybackTime = e.AddedPlayback.Messages.Values.Select(x => x.Message.ScheduledAt).OrderBy(x => x).First();

        lock (_lock)
        {
            if (_nextRunTime.HasValue && newPlaybackTime < _nextRunTime.Value.ToUnixTimeMilliseconds())
            {
                // When a new playback is added or the new time is earlier than the previously calculated next run time, update the next run time and set the signal
                _nextRunTime = DateTimeOffset.FromUnixTimeMilliseconds(newPlaybackTime);
                _signalEvent.Set();
            }
        }
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var timeUntilTrigger = TimeSpan.Zero;

                lock (_lock)
                {
                    var triggerTime = _playbacksRepository.GetNearestRunTime();
                    timeUntilTrigger = triggerTime.HasValue ? triggerTime.Value - _systemClock.UtcNow : _idleWaitTime;
                    _nextRunTime = _systemClock.UtcNow + timeUntilTrigger;
                }

                if (timeUntilTrigger > TimeSpan.Zero)
                {
                    WaitWithTimeout(timeUntilTrigger, cancellationToken);
                }

                await HandleMessagesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in schedule main loop");
            }
        }
    }

    private void WaitWithTimeout(TimeSpan timeout, CancellationToken cancellationToken)
    {
        _signalEvent.Wait(timeout, cancellationToken);
        _signalEvent.Reset();
    }

    private async Task HandleMessagesAsync(CancellationToken cancellationToken)
    {
        foreach (var message in _playbacksRepository.GetMessagesToExecute())
        {
            _playbacksRepository.RemoveMessageFromPlayback(message.PlaybackId, message);

            var actionBlock = _actionBlocks.GetOrAdd(
                message.PlaybackId,
                CreatExecutionBlock(cancellationToken));

            await actionBlock.SendAsync(message);
        }
    }

    private ActionBlock<PlaybackItem> CreatExecutionBlock(CancellationToken cancellationToken)
    {
        return new ActionBlock<PlaybackItem>(
                async message =>
                {
                    try
                    {
                        var handler = _messageHandlerResolver.Resolve(message.Message.MessageType);

                        await handler.Handle(message.Message, message.PlaybackId, cancellationToken);

                        _logger.LogInformation($"{message.InternalId} published at: {DateTimeOffset.UtcNow}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message);
                    }
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 1,
                    BoundedCapacity = 1000,
                    EnsureOrdered = true
                });
    }
}
