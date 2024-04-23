namespace Beter.TestingTool.Generator.Domain.Playbacks;

public sealed record Playback
{
    public string Id { get; set; }
    public int CaseId { get; set; }
    public Version Version { get; set; }
    public string Description { get; set; }
    public DateTime StartedAt { get; set; }
    public long LastMessageSentAt { get; init; }
    public long ActiveMessagesCount { get; init; }
    public IDictionary<string, PlaybackItem> Messages { get; init; }
}
