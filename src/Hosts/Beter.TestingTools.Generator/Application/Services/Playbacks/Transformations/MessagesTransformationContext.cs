using Beter.TestingTool.Generator.Application.Contracts.Playbacks;

namespace Beter.TestingTool.Generator.Application.Services.Playbacks.Transformations;

public sealed class MessagesTransformationContext
{
    public sealed class MatchIdProfile
    {
        public string Id { get; set; }
        public string NewId { get; set; }
        public DateTime OldStartDate { get; set; }
        public Dictionary<string, DateTime> OldFirstTimestampByEachMessageType { get; set; } = [];
        public bool WasFirstTimeTableMessage { get; set; }
        public bool IsFirstTimeTableMessageDelayProcessed { get; set; }
    }

    public ReplyMode ReplyMode { get; set; }
    public int CaseId { get; set; }
    public int RunCount { get; set; }
    public DateTime TestCaseStart { get; set; }
    public TimeSpan TimeOffsetAfterFirstTimetableMessageInSecounds { get; set; }
    public double AccelerationFactor { get; set; }
    public DateTime OldFirstMessageScheduledAt { get; set; }
    public DateTime NewFirstMessageScheduledAt { get; set; }
    public Dictionary<string, MatchIdProfile> Matches { get; set; }

    public MatchIdProfile GetMatchProfile(string matchId)
    {
        if (Matches.TryGetValue(matchId, out var profile))
        {
            return profile;
        }

        profile = Matches.Select(m => m.Value).FirstOrDefault(m => m.NewId == matchId);
        if (profile == null)
        {
            throw new ArgumentException($"Profile for Match ID: {matchId} does not exist.");
        }

        return profile;
    }
}