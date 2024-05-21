using Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;

namespace Beter.Feed.TestingSandbox.Generator.Application.Contracts.Playbacks;

public interface IPlaybackRepository
{
    IEnumerable<PlaybackItem> GetMessagesToExecute();
    DateTimeOffset? GetNearestRunTime();
    Playback Get(Guid playbackId);
    IEnumerable<Playback> GetActive();
    void Add(Playback playback);
    Playback Remove(Guid playbackId);
    IEnumerable<Playback> RemoveAll();
    Playback RemoveMessageFromPlayback(Guid playbackId, PlaybackItem messageToRemove);

    event EventHandler<PlaybackEventArgs> PlaybackAdded;
}

public class PlaybackEventArgs : EventArgs
{
    public Playback AddedPlayback { get; }

    public PlaybackEventArgs(Playback addedPlayback)
    {
        AddedPlayback = addedPlayback ?? throw new ArgumentNullException(nameof(addedPlayback));
    }
}
