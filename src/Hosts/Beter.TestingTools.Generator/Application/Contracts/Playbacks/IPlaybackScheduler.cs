namespace Beter.TestingTool.Generator.Application.Contracts.Playbacks;

public interface IPlaybackScheduler
{
    Task RunAsync(CancellationToken cancellationToken);
}