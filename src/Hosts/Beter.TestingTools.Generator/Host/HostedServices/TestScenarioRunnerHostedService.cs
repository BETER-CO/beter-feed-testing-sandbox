using Beter.TestingTools.Generator.Application.Contracts.Playbacks;

namespace Beter.TestingTools.Generator.Host.HostedServices;

public class TestScenarioRunnerHostedService : IHostedService
{
    private Task _executingTask;
    private CancellationTokenSource _cts;

    private readonly IPlaybackScheduler _scheduler;

    public TestScenarioRunnerHostedService(IPlaybackScheduler scheduler)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _executingTask = ExecuteAsync(_cts.Token);

        return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();

        Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

        cancellationToken.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }

    private Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(async () => await _scheduler.RunAsync(cancellationToken), TaskCreationOptions.LongRunning);
    }
}

