using Beter.Feed.TestingSandbox.Common.Models;

namespace Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;

public sealed record Playback
{
    public string Id { get; set; }
    public int CaseId { get; set; }
    public Version Version { get; set; }
    public string Description { get; set; }
    public DateTime StartedAt { get; set; }
    public long LastMessageSentAt { get; init; }
    public long ActiveMessagesCount { get => Messages.Count; }
    public AdditionalInfo AdditionInfo { get; set; } = AdditionalInfo.NoInfo();
    public Dictionary<string, PlaybackItem> Messages { get; set; } = new Dictionary<string, PlaybackItem>();
}
