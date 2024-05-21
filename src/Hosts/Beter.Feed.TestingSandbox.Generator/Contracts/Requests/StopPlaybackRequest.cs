namespace Beter.Feed.TestingSandbox.Generator.Contracts.Requests;

public record StopPlaybackRequest
{
    public Guid PlaybackId { get; set; }

    public StopPlaybackCommand Command { get; set; }
}

public enum StopPlaybackCommand
{
    StopSingle = 1,
    StopAll = 2
}
