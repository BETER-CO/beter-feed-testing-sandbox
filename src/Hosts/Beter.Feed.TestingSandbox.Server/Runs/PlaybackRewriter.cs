#nullable enable

using System.Globalization;
using System.Text.Json;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Common.Serialization;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Models.TradingInfos;

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>
/// The rewrite of one run: it turns one message entry of a case script into what the hubs
/// send. It gives every recorded match id one new GUID for the run, numbers the offsets from
/// the counter of the container, and moves the recorded timestamps onto the run's clock.
/// It does no I/O; the player calls it for one entry at a time, in send order.
/// </summary>
internal sealed class PlaybackRewriter
{
    private readonly long _startedAt;
    private readonly double _acceleration;
    private readonly OffsetSequence _offsets;
    private readonly IReadOnlyDictionary<(string matchId, string channel), long> _bases;
    private readonly Dictionary<string, string> _matchIds = new(StringComparer.Ordinal);
    private readonly Dictionary<(string matchId, string channel), long> _lastOffsets = new();

    /// <summary>Initializes a new instance of the <see cref="PlaybackRewriter"/> class.</summary>
    /// <param name="startedAt">The instant the run's script clock started.</param>
    /// <param name="acceleration">The run's acceleration.</param>
    /// <param name="offsets">The offset counter of the container.</param>
    /// <param name="bases">The recorded timestamp each match starts from on each channel, <see cref="CaseScript.Bases"/>.</param>
    public PlaybackRewriter(
        DateTimeOffset startedAt,
        double acceleration,
        OffsetSequence offsets,
        IReadOnlyDictionary<(string matchId, string channel), long> bases)
    {
        _startedAt = startedAt.ToUnixTimeMilliseconds();
        _acceleration = acceleration;
        _offsets = offsets ?? throw new ArgumentNullException(nameof(offsets));
        _bases = bases ?? throw new ArgumentNullException(nameof(bases));
    }

    /// <summary>Gets the new id of every recorded match id met so far.</summary>
    public IReadOnlyDictionary<string, string> MatchIds => _matchIds;

    /// <summary>Rewrites one message entry.</summary>
    /// <param name="entry">The entry.</param>
    /// <returns>What the hub of the entry's channel sends.</returns>
    /// <exception cref="InvalidDataException">The entry cannot be played: its value does not deserialize, or its <c>messageType</c> is not supported.</exception>
    public PlaybackMessage Rewrite(ScriptEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var channel = entry.Channel ?? throw new InvalidDataException("a directive is not a message");
        return entry.MessageType switch
        {
            ChannelNames.Timetable => Update(entry, Read<TimeTableItemModel[]>(entry), TimeTable),
            ChannelNames.Trading => Update(entry, Read<TradingInfoModel[]>(entry), Trading),
            ChannelNames.Scoreboard => Update(entry, Read<ScoreBoardModel[]>(entry), Scoreboard),
            ChannelNames.Incident => Update(entry, Read<IncidentModel[]>(entry), Incident),
            MessageTypes.SystemEvent => SystemEvent(channel, Read<GlobalMessageModel[]>(entry)),
            MessageTypes.SubscriptionsRemoved => SubscriptionsRemoved(channel, Read<SubscriptionsRemovedModel>(entry)),
            _ => throw new InvalidDataException($"unsupported messageType '{entry.MessageType}'"),
        };
    }

    private static T Read<T>(ScriptEntry entry)
        where T : class
    {
        try
        {
            return JsonHubSerializer.Deserialize<T>(entry.Value)
                ?? throw new InvalidDataException($"the value of a {entry.MessageType} entry is null");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"the value does not deserialize as {entry.MessageType}: {exception.Message}", exception);
        }
        catch (NotSupportedException exception)
        {
            throw new InvalidDataException($"the value does not deserialize as {entry.MessageType}: {exception.Message}", exception);
        }
    }

    /// <summary>
    /// Checks that every item carries an <c>id</c> and the field its time is read from: a
    /// missing one would otherwise read as 0, or as the first instant of the calendar.
    /// </summary>
    private static void RequireFields(JsonElement items, string timeField)
    {
        var index = 0;
        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.String)
            {
                throw new InvalidDataException(string.Create(CultureInfo.InvariantCulture, $"item {index} has no id"));
            }

            if (!item.TryGetProperty(timeField, out var time) || time.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                throw new InvalidDataException(string.Create(CultureInfo.InvariantCulture, $"item {index} has no {timeField}"));
            }

            index++;
        }
    }

    private static DateTimeOffset AsUtc(DateTime value) =>
        new(value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime());

    private PlaybackMessage Update<T>(ScriptEntry entry, T[] items, Action<T, string> rewrite)
        where T : class, IFeedMessage
    {
        var channel = entry.Channel!;
        if (channel != entry.MessageType)
        {
            throw new InvalidDataException($"messageType '{entry.MessageType}' is not sent on channel '{channel}'");
        }

        RequireFields(entry.Value, channel == ChannelNames.Incident ? "date" : "timestamp");
        foreach (var item in items)
        {
            if (item is null)
            {
                throw new InvalidDataException("an item of the value is null");
            }

            var recordedId = item.Id;
            rewrite(item, recordedId);
            item.Id = NewId(recordedId);
            item.Offset = OffsetOf(item.Id, channel, item.MsgType);
        }

        return new PlaybackMessage(channel, HubMethods.OnUpdate, items);
    }

    private void TimeTable(TimeTableItemModel item, string recordedId)
    {
        var start = Base(recordedId, ChannelNames.Timetable, item.Timestamp);
        item.Timestamp = Shift(item.Timestamp, start);
        if (item.StartDate is { } startDate)
        {
            item.StartDate = DateTimeOffset.FromUnixTimeMilliseconds(Shift(AsUtc(startDate).ToUnixTimeMilliseconds(), start)).UtcDateTime;
        }
    }

    private void Trading(TradingInfoModel item, string recordedId) =>
        item.Timestamp = Shift(item.Timestamp, Base(recordedId, ChannelNames.Trading, item.Timestamp));

    private void Scoreboard(ScoreBoardModel item, string recordedId)
    {
        item.Timestamp = Shift(item.Timestamp, Base(recordedId, ChannelNames.Scoreboard, item.Timestamp));
        if (item.Timer is { } timer)
        {
            timer.TimeStamp = item.Timestamp;
        }
    }

    private void Incident(IncidentModel item, string recordedId)
    {
        var recorded = AsUtc(item.Date).ToUnixTimeMilliseconds();
        item.Date = DateTimeOffset.FromUnixTimeMilliseconds(Shift(recorded, Base(recordedId, ChannelNames.Incident, recorded))).UtcDateTime;
    }

    private PlaybackMessage SystemEvent(string channel, GlobalMessageModel[] events)
    {
        foreach (var item in events)
        {
            if (item is null)
            {
                throw new InvalidDataException("an item of the value is null");
            }

            item.Id = NewId(item.Id);
        }

        return new PlaybackMessage(channel, HubMethods.OnSystemEvent, events);
    }

    private PlaybackMessage SubscriptionsRemoved(string channel, SubscriptionsRemovedModel removed)
    {
        var ids = (removed.Ids ?? throw new InvalidDataException("the value carries no ids")).Select(NewId).ToArray();
        return new PlaybackMessage(channel, HubMethods.OnSubscriptionsRemove, ids);
    }

    /// <summary>The new id of a recorded match id; null stays null.</summary>
    private string NewId(string recordedId)
    {
        if (recordedId is null)
        {
            return null!;
        }

        if (!_matchIds.TryGetValue(recordedId, out var id))
        {
            id = Guid.NewGuid().ToString("D");
            _matchIds.Add(recordedId, id);
        }

        return id;
    }

    /// <summary>
    /// An <c>Update</c> takes the next number and keeps it for the match on the channel; any
    /// other <c>msgType</c> repeats the kept number, or takes the next one when none is kept.
    /// </summary>
    private long OffsetOf(string matchId, string channel, int msgType)
    {
        var key = (matchId ?? string.Empty, channel);
        if (msgType != (int)MessageType.Update && _lastOffsets.TryGetValue(key, out var kept))
        {
            return kept;
        }

        var next = _offsets.Next();
        _lastOffsets[key] = next;
        return next;
    }

    private long Base(string recordedId, string channel, long recorded) =>
        recordedId is not null && _bases.TryGetValue((recordedId, channel), out var start) ? start : recorded;

    private long Shift(long recorded, long start) => _startedAt + (long)Math.Round((recorded - start) / _acceleration);
}
