using Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;

namespace Beter.Feed.TestingSandbox.Generator.Application.Contracts.Playbacks;

public interface IPlaybackFactory
{
    Playback Create(
        int caseId,
        ReplyMode replyMode,
        int timeOffsetInMinutes,
        int? timeOffsetBetweenMessagesInSecounds,
        int timeOffsetAfterFirstTimetableMessageInSecounds,
        double accelerationFactor);
}

public enum ReplyMode
{
    FixedDelay = 1,
    HistoricalTimeline = 2
}
