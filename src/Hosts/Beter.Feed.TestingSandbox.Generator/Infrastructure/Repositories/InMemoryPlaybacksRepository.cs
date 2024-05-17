using Beter.Feed.TestingSandbox.Generator.Application.Contracts;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Application.Exceptions;
using Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;
using System.Collections.Concurrent;

namespace Beter.Feed.TestingSandbox.Generator.Infrastructure.Repositories;

public sealed class InMemoryPlaybacksRepository : IPlaybackRepository
{
    private readonly ConcurrentDictionary<string, Playback> _playbacks;
    private readonly ISystemClock _systemClock;

    public event EventHandler<PlaybackEventArgs> PlaybackAdded;

    public InMemoryPlaybacksRepository(ISystemClock systemClock)
    {
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _playbacks = new ConcurrentDictionary<string, Playback>();
    }

    public void Add(Playback playback)
    {
        if (playback == null)
            throw new ArgumentNullException(nameof(playback));

        if (_playbacks.ContainsKey(playback.Id))
            throw new DuplicateEntityException($"The playback with with given ID: '{playback.Id}' already exists.");

        _playbacks[playback.Id] = playback;

        PlaybackAdded?.Invoke(this, new PlaybackEventArgs(playback));
    }

    public Playback Remove(string playbackId)
    {
        if (playbackId == null)
            throw new ArgumentNullException(nameof(playbackId));

        if (!_playbacks.TryRemove(playbackId, out var playback))
            throw new RequiredEntityNotFoundException($"The playback with the specified ID: '{playbackId}' does not exist.");

        return playback;
    }

    public IEnumerable<Playback> RemoveAll()
    {
        var result = new List<Playback>();
        foreach (var playback in _playbacks)
        {
            result.Add(Remove(playback.Key));
        }

        return result;
    }

    public Playback Get(string playbackId)
    {
        _playbacks.TryGetValue(playbackId, out var playback);

        return playback;
    }

    public IEnumerable<Playback> GetActive()
    {
        return _playbacks
            .Where(playback => playback.Value.ActiveMessagesCount > 0)
            .Select(playback => playback.Value)
            .ToList();
    }

    public Playback RemoveMessageFromPlayback(string playbackId, PlaybackItem messageToRemove)
    {
        var playback = Get(playbackId);

        playback = playback with
        {
            LastMessageSentAt = messageToRemove.Message.ScheduledAt,
            Messages = RemoveMessages(
               [messageToRemove.InternalId],
               playback.Messages)
        };

        return Replace(playback);
    }

    private Playback Replace(Playback playback)
    {
        if (playback == null)
            throw new ArgumentNullException(nameof(playback));

        if (!_playbacks.ContainsKey(playback.Id))
            throw new RequiredEntityNotFoundException($"The playback with the specified ID: '{playback.Id}' does not exist.");

        return _playbacks[playback.Id] = playback;
    }

    private static Dictionary<string, PlaybackItem> RemoveMessages(
      IEnumerable<string> messageIdsToRemove,
      Dictionary<string, PlaybackItem> actualMessages)
    {
        foreach (var messageId in messageIdsToRemove)
        {
            actualMessages.Remove(messageId);
        }

        return actualMessages;
    }

    public DateTimeOffset? GetNearestRunTime()
    {
        var scheduledAtDates = GetActive()
            .SelectMany(x => x.Messages)
            .Select(message => message.Value.Message.ScheduledAt);

        return scheduledAtDates.Any()
            ? DateTimeOffset.FromUnixTimeMilliseconds(scheduledAtDates.Min())
            : null;
    }

    public IEnumerable<PlaybackItem> GetMessagesToExecute()
    {
        var messagesToExecute = GetActive()
                .SelectMany(x => x.Messages)
                .Select(x => x.Value)
                .Where(message => message.Message.ScheduledAt <= _systemClock.UtcNow.ToUnixTimeMilliseconds())
                .OrderBy(message => message.Message.ScheduledAt)
                .ToList();

        return messagesToExecute;
    }
}