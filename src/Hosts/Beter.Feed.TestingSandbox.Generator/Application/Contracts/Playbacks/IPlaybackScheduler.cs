namespace Beter.Feed.TestingSandbox.Generator.Application.Contracts.Playbacks;

public interface IPlaybackScheduler
{
    Task RunAsync(CancellationToken cancellationToken);
}