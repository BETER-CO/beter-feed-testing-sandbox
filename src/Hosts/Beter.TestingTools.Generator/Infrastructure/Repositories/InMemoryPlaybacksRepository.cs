using Beter.TestingTool.Generator.Application.Contracts;
using Beter.TestingTool.Generator.Application.Contracts.Playbacks;
using Beter.TestingTool.Generator.Domain.Playbacks;
using System.Collections.Concurrent;

namespace Beter.TestingTool.Generator.Infrastructure.Repositories;

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
            throw new ArgumentException($"Playback with ID {playback.Id} already exists.");

        _playbacks[playback.Id] = playback;

        PlaybackAdded?.Invoke(this, new PlaybackEventArgs(playback));
    }

    public Playback Remove(string playbackId)
    {
        if (playbackId == null)
            throw new ArgumentNullException(nameof(playbackId));

        if (!_playbacks.TryRemove(playbackId, out var playback))
            throw new KeyNotFoundException($"Playback with ID {playbackId} does not exist.");

        return playback;
    }

    public IEnumerable<Playback> RemoveAll()
    {
        foreach (var playback in _playbacks)
        {
            yield return Remove(playback.Key);
        }
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

    public void RemoveMessageFromPlayback(string playbackId, PlaybackItem messageToRemove)
    {
        var playback = Get(playbackId);

        playback = playback with
        {
            LastMessageSentAt = messageToRemove.Message.ScheduledAt,
            ActiveMessagesCount = playback.ActiveMessagesCount - 1,
            Messages = RemoveMessages(
               [messageToRemove.InternalId],
               playback.Messages)
        };

        Replace(playback);
    }

    private void Replace(Playback playback)
    {
        if (playback == null)
            throw new ArgumentNullException(nameof(playback));

        if (!_playbacks.ContainsKey(playback.Id))
            throw new KeyNotFoundException($"Playback with ID {playback.Id} does not exist.");

        _playbacks[playback.Id] = playback;
    }

    private static IDictionary<string, PlaybackItem> RemoveMessages(
      IEnumerable<string> messageIdsToRemove,
      IDictionary<string, PlaybackItem> actualMessages)
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