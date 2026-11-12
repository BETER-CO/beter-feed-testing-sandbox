#nullable enable

using System.Globalization;
using System.Text.Json;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Server.Cases;

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>
/// The script of one case, read for one run: the entries in the order they are sent, and the
/// recorded timestamp each match starts from on each channel. It holds the parsed case file
/// until it is disposed.
/// </summary>
internal sealed class CaseScript : IDisposable
{
    private readonly JsonDocument _document;

    private CaseScript(JsonDocument document, IReadOnlyList<ScriptEntry> timeline, IReadOnlyDictionary<(string matchId, string channel), long> bases)
    {
        _document = document;
        Timeline = timeline;
        Bases = bases;
    }

    /// <summary>
    /// Gets the entries in send order: by <c>at</c>, and by script position for equal
    /// <c>at</c>. A directive that waits for an event in place of an offset is left out.
    /// </summary>
    public IReadOnlyList<ScriptEntry> Timeline { get; }

    /// <summary>
    /// Gets the recorded timestamp, in Unix milliseconds, of the first item of each recorded
    /// match on each channel, in script order: <c>timestamp</c>, or <c>date</c> on incident.
    /// </summary>
    public IReadOnlyDictionary<(string matchId, string channel), long> Bases { get; }

    /// <summary>Reads the script of a case file.</summary>
    /// <param name="entry">The case.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The script.</returns>
    /// <exception cref="InvalidDataException">The case has no file, or the file holds no playable script.</exception>
    public static async Task<CaseScript> LoadAsync(CaseEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var file = entry.OpenFile() ?? throw new InvalidDataException($"Case {entry.CaseId} has no case file.");
        await using (file)
        {
            JsonDocument document;
            try
            {
                document = await JsonDocument.ParseAsync(file, cancellationToken: cancellationToken);
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException($"The case file of {entry.CaseId} is not JSON: {exception.Message}", exception);
            }

            return Parse(document);
        }
    }

    /// <summary>Reads the script of a parsed case file, which the script then owns.</summary>
    /// <param name="document">The case file.</param>
    /// <returns>The script.</returns>
    /// <exception cref="InvalidDataException">The file holds no playable script.</exception>
    public static CaseScript Parse(JsonDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        try
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty("script", out var script)
                || script.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException("The case file has no script.");
            }

            var entries = script.EnumerateArray().Select(EntryOf).ToArray();
            var timeline = entries.Where(entry => entry is not null).Select(entry => entry!).OrderBy(entry => entry.At).ToArray();
            return new CaseScript(document, timeline, BasesOf(entries));
        }
        catch
        {
            document.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public void Dispose() => _document.Dispose();

    private static ScriptEntry? EntryOf(JsonElement entry, int index)
    {
        if (entry.ValueKind != JsonValueKind.Object)
        {
            throw Defect(index, "the entry is not an object");
        }

        var hasAt = entry.TryGetProperty("at", out var at);
        if (hasAt && (at.ValueKind != JsonValueKind.Number || !at.TryGetInt64(out _) || at.GetInt64() < 0))
        {
            throw Defect(index, "at is not a whole number of milliseconds from 0 up");
        }

        if (entry.TryGetProperty("directive", out var directive))
        {
            return hasAt ? new ScriptEntry(index, at.GetInt64(), null, null, directive.GetString() ?? string.Empty, default) : null;
        }

        if (!hasAt
            || !entry.TryGetProperty("channel", out var channel) || channel.ValueKind != JsonValueKind.String
            || !entry.TryGetProperty("messageType", out var messageType) || messageType.ValueKind != JsonValueKind.String
            || !entry.TryGetProperty("value", out var value))
        {
            throw Defect(index, "a message entry carries at, channel, messageType and value");
        }

        return new ScriptEntry(index, at.GetInt64(), channel.GetString(), messageType.GetString(), null, value);
    }

    private static Dictionary<(string matchId, string channel), long> BasesOf(IEnumerable<ScriptEntry?> inScriptOrder)
    {
        var bases = new Dictionary<(string matchId, string channel), long>();
        foreach (var entry in inScriptOrder)
        {
            if (entry is null || entry.IsDirective || entry.Value.ValueKind != JsonValueKind.Array
                || entry.MessageType is not (ChannelNames.Timetable or ChannelNames.Trading or ChannelNames.Scoreboard or ChannelNames.Incident))
            {
                continue;
            }

            foreach (var item in entry.Value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object
                    && item.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String
                    && RecordedTimeOf(item, entry.MessageType) is { } recorded)
                {
                    bases.TryAdd((id.GetString()!, entry.MessageType), recorded);
                }
            }
        }

        return bases;
    }

    private static long? RecordedTimeOf(JsonElement item, string channel)
    {
        if (channel == ChannelNames.Incident)
        {
            return item.TryGetProperty("date", out var date) && date.ValueKind == JsonValueKind.String
                && DateTimeOffset.TryParse(date.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
                ? parsed.ToUnixTimeMilliseconds()
                : null;
        }

        return item.TryGetProperty("timestamp", out var timestamp) && timestamp.ValueKind == JsonValueKind.Number
            && timestamp.TryGetInt64(out var milliseconds)
            ? milliseconds
            : null;
    }

    private static InvalidDataException Defect(int index, string cause) =>
        new(string.Create(CultureInfo.InvariantCulture, $"script entry {index}: {cause}."));
}
