namespace Beter.TestingTools.Generator.Application.Contracts.Playbacks;

public interface IPlaybackScheduler
{
    Task RunAsync(CancellationToken cancellationToken);
}