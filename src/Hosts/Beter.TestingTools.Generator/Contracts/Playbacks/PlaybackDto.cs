namespace Beter.TestingTool.Generator.Contracts.Playbacks;

public sealed record PlaybackDto
{
    public string Id { get; set; }
    public int CaseId { get; set; }
    public string Version { get; set; }
    public string Description { get; set; }
    public DateTime StartedAt { get; set; }
    public long LastMessageSentAt { get; init; }
    public long ActiveMessagesCount { get; init; }
}