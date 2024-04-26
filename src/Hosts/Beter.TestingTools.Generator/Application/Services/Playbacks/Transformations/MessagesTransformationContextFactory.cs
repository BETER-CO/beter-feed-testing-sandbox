using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models;
using Beter.TestingTools.Models.Incidents;
using Beter.TestingTools.Models.Scoreboards;
using Beter.TestingTools.Models.TimeTableItems;
using Beter.TestingTools.Models.TradingInfos;
using static Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.MessagesTransformationContext;
using Beter.TestingTools.Generator.Application.Contracts;
using Beter.TestingTools.Generator.Application.Contracts.Playbacks;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.Application.Extensions;

namespace Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations;

public interface IMessagesTransformationContextFactory
{
    MessagesTransformationContext Create(
        int testCaseId,
        ReplyMode replyMode,
        IEnumerable<TestScenarioMessage> messages,
        TimeSpan offset,
        TimeSpan timeOffsetAfterFirstTimetableMessageInSecounds,
        double accelerationFactor = 1);
}

public class MessagesTransformationContextFactory : IMessagesTransformationContextFactory
{
    private readonly ISystemClock _systemClock;
    private readonly IRunCountTracker _runCountTracker;
    private readonly IMatchIdGenerator _matchIdGenerator;

    public MessagesTransformationContextFactory(ISystemClock systemClock, IRunCountTracker runCountTracker, IMatchIdGenerator matchIdGenerator)
    {
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _runCountTracker = runCountTracker ?? throw new ArgumentNullException(nameof(runCountTracker));
        _matchIdGenerator = matchIdGenerator ?? throw new ArgumentNullException(nameof(matchIdGenerator));
    }

    public MessagesTransformationContext Create(int testCaseId, ReplyMode replyMode, IEnumerable<TestScenarioMessage> messages, TimeSpan offset, TimeSpan timeOffsetAfterFirstTimetableMessageInSecounds, double accelerationFactor = 1)
    {
        ValidateMessages(messages);

        var runCount = _runCountTracker.GetNext();
        var testCaseStart = _systemClock.UtcNow.Add(offset).UtcDateTime;
        var oldFirstMessageScheduledAt = GetOldFirstMessageScheduledAt(messages);
        var newFirstMessageScheduledAt = testCaseStart.ToUnixTimeMilliseconds();

        var matches = CreateMatches(
            messages,
            testCaseId,
            runCount,
            oldFirstMessageScheduledAt,
            newFirstMessageScheduledAt,
            timeOffsetAfterFirstTimetableMessageInSecounds,
            accelerationFactor);

        return new MessagesTransformationContext
        {
            CaseId = testCaseId,
            ReplyMode = replyMode,
            AccelerationFactor = accelerationFactor,
            RunCount = runCount,
            TestCaseStart = testCaseStart,
            TimeOffsetAfterFirstTimetableMessageInSecounds = timeOffsetAfterFirstTimetableMessageInSecounds,
            OldFirstMessageScheduledAt = oldFirstMessageScheduledAt,
            NewFirstMessageScheduledAt = newFirstMessageScheduledAt,
            Matches = matches
        };
    }

    private static void ValidateMessages(IEnumerable<TestScenarioMessage> messages)
    {
        if (!messages.Any())
        {
            throw new ArgumentException($"Test scenario should have at least one message.");
        }
    }

    private static long GetOldFirstMessageScheduledAt(IEnumerable<TestScenarioMessage> messages)
    {
        return messages.First().ScheduledAt;
    }

    private Dictionary<string, MatchIdProfile> CreateMatches(
        IEnumerable<TestScenarioMessage> messages,
        int testCaseId,
        int runCount,
        long oldFirstMessageScheduledAt,
        long newFirstMessageScheduledAt,
        TimeSpan timeOffsetAfterFirstTimetableMessageInSecounds,
        double accelerationFactor)
    {
        static Dictionary<string, DateTime> GetOldStartDateForEachMatchId(IEnumerable<TestScenarioMessage> messages)
        {
            return messages.Where(x => x.IsMessageType(MessageTypes.Timetable))
                .SelectMany(message => message.GetValue<IEnumerable<TimeTableItemModel>>())
                .GroupBy(item => item.Id, item => item)
                .ToDictionary(x => x.Key, x => DateTime.SpecifyKind(x.Min(item => item.StartDate.Value), DateTimeKind.Utc));
        }

        MatchIdProfile CreateMatchProfile(string matchId, int testCaseId, int runCount, DateTime oldStartDate, Dictionary<string, DateTime> oldFirstTimestamp)
        {
            var profile = new MatchIdProfile
            {
                Id = matchId,
                NewId = _matchIdGenerator.Generate(testCaseId, runCount),
                OldStartDate = oldStartDate,
                NewStartDate = CalculateNewStartDate(oldStartDate),
                OldFirstTimestampByEachMessageType = oldFirstTimestamp
            };

            return profile;
        }

        DateTime CalculateNewStartDate(DateTime oldStartDate)
        {
            var newFirstMessageScheduledAtDate = DateTimeOffset.FromUnixTimeMilliseconds(newFirstMessageScheduledAt).UtcDateTime;
            var oldFirstMessageScheduledAtDate = DateTimeOffset.FromUnixTimeMilliseconds(oldFirstMessageScheduledAt).UtcDateTime;

            return oldStartDate
                + newFirstMessageScheduledAtDate.Subtract(oldStartDate)
                + oldStartDate.Subtract(oldFirstMessageScheduledAtDate) / accelerationFactor
                + timeOffsetAfterFirstTimetableMessageInSecounds;

        }

        var oldStartDate = GetOldStartDateForEachMatchId(messages);
        var oldFirstTimestamp = CreateOldFirstTimestampForEachMatchIdAndChannel(messages);

        return GetDistinctMatchIds(messages)
            .ToDictionary(
                matchId => matchId,
                matchId => CreateMatchProfile(matchId, testCaseId, runCount, oldStartDate[matchId], oldFirstTimestamp[matchId]));
    }

    private static Dictionary<string, Dictionary<string, DateTime>> CreateOldFirstTimestampForEachMatchIdAndChannel(IEnumerable<TestScenarioMessage> messages)
    {
        static void AddMessageType<T>(IEnumerable<TestScenarioMessage> messages, string messageType, string matchId, Dictionary<string, DateTime> messageTypes, Func<T, DateTime> getDateFunc) where T : IIdentityModel
        {
            var message = messages
                .Where(x => x.MessageType == messageType)
                .SelectMany(x => x.GetValue<IEnumerable<T>>())
                .FirstOrDefault(x => x.Id == matchId);

            if (message != null)
            {
                messageTypes.Add(messageType, getDateFunc(message));
            }
        }

        static Dictionary<string, DateTime> GetMessageTypesForMatchId(IEnumerable<TestScenarioMessage> messages, string matchId)
        {
            var messageTypes = new Dictionary<string, DateTime>();

            AddMessageType<TimeTableItemModel>(messages, MessageTypes.Timetable, matchId, messageTypes, x => x.Timestamp.ToUtcDateTime());
            AddMessageType<ScoreBoardModel>(messages, MessageTypes.Scoreboard, matchId, messageTypes, x => x.Timestamp.ToUtcDateTime());
            AddMessageType<TradingInfoModel>(messages, MessageTypes.Trading, matchId, messageTypes, x => x.Timestamp.ToUtcDateTime());
            AddMessageType<IncidentModel>(messages, MessageTypes.Incident, matchId, messageTypes, x => DateTime.SpecifyKind(x.Date, DateTimeKind.Utc));

            return messageTypes;
        }

        return GetDistinctMatchIds(messages)
            .ToDictionary(
                matchId => matchId,
                matchId => GetMessageTypesForMatchId(messages, matchId));
    }

    private static IEnumerable<string> GetDistinctMatchIds(IEnumerable<TestScenarioMessage> messages)
    {
        return messages
            .Where(x => x.MessageType == MessageTypes.Scoreboard || x.MessageType == MessageTypes.Incident || x.MessageType == MessageTypes.Timetable || x.MessageType == MessageTypes.Trading)
            .SelectMany(x => x.ToFeedMessages())
            .Select(x => x.Id)
            .Distinct();
    }
}
