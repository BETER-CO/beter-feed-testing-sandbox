using System.Text.Json;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Models.GlobalEvents;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Beter.Feed.TestingSandbox.Server.Runs;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Runs;

/// <summary>
/// The rewrite of the ids, offsets and timestamps of one run, on a script of one recorded
/// match "m1" whose first items carry the recorded times below.
/// </summary>
public class PlaybackRewriterTests
{
    private const long TimeTableBase = 1_720_000_000_000;
    private const long TradingBase = 1_720_000_000_100;
    private const long ScoreboardBase = 1_720_000_000_500;

    // 2024-07-03T09:46:40.000Z is TimeTableBase.
    private const string Script = """
        {
          "script": [
            { "at": 0, "channel": "time_table", "messageType": "time_table", "value": [
              { "id": "m1", "sportId": 6, "status": 1, "startDate": "2024-07-03T10:46:40Z", "timestamp": 1720000000000, "msgType": 1, "offset": 70, "name": { "en": "A vs B" } } ] },
            { "at": 100, "channel": "trading", "messageType": "trading", "value": [
              { "id": "m1", "sportId": 6, "tradingStatus": 1, "markets": [ { "id": "mk", "outcomes": [ { "id": "o", "price": 1.5, "prices": { "decimal": 1.53, "american": "-188" } } ] } ], "timestamp": 1720000000100, "msgType": 1, "offset": 71 } ] },
            { "at": 500, "channel": "scoreboard", "messageType": "scoreboard", "value": [
              { "id": "m1", "sportId": 6, "stage": 2, "timer": { "timeStamp": 5, "timerValue": 300, "enabled": true }, "timestamp": 1720000000500, "msgType": 1, "offset": 72 } ] },
            { "at": 900, "channel": "incident", "messageType": "incident", "value": [
              { "id": "m1", "sportId": 6, "index": 3, "type": "1029", "date": "2024-07-03T09:46:40.900Z", "msgType": 1, "offset": 73 } ] },
            { "at": 1000, "channel": "time_table", "messageType": "time_table", "value": [
              { "id": "m1", "status": 2, "startDate": "2024-07-03T10:46:42Z", "timestamp": 1720000002000, "msgType": 2, "offset": 74 } ] },
            { "at": 1000, "channel": "trading", "messageType": "trading", "value": [
              { "id": "m1", "timestamp": 1720000004100, "msgType": 1, "offset": 75 },
              { "id": "m1", "timestamp": 1720000004200, "msgType": 3, "offset": 76 } ] },
            { "at": 2000, "channel": "scoreboard", "messageType": "scoreboard", "value": [
              { "id": "m1", "timestamp": 1720000002500, "msgType": 1, "offset": 77 } ] },
            { "at": 2000, "channel": "incident", "messageType": "incident", "value": [
              { "id": "m1", "index": 4, "type": "1", "date": "2024-07-03T09:46:42.900Z", "msgType": 1, "offset": 78 } ] },
            { "at": 3000, "channel": "incident", "messageType": "system_event", "value": [ { "id": "m1", "eventType": 1, "eventValue": 1 }, { "id": "m2", "eventType": 1, "eventValue": 2 } ] },
            { "at": 3000, "channel": "trading", "messageType": "subscriptions_removed", "value": { "ids": ["m1", "m3"] } },
            { "at": 4000, "channel": "time_table", "messageType": "time_table", "value": [ { "id": "m1", "timestamp": 1720000005000, "msgType": 1 } ] }
          ]
        }
        """;

    private static readonly DateTimeOffset StartedAt = new(2026, 11, 12, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Script_IsOrderedByAt_AndTakesTheBaseOfEachMatchAndChannelFromTheFirstItem()
    {
        using var script = CaseScript.Parse(JsonDocument.Parse(Script));

        Assert.Equal(Enumerable.Range(0, 11), script.Timeline.Select(entry => entry.Index));
        Assert.Equal(TimeTableBase, script.Bases[("m1", ChannelNames.Timetable)]);
        Assert.Equal(TradingBase, script.Bases[("m1", ChannelNames.Trading)]);
        Assert.Equal(ScoreboardBase, script.Bases[("m1", ChannelNames.Scoreboard)]);
        Assert.Equal(TimeTableBase + 900, script.Bases[("m1", ChannelNames.Incident)]);
    }

    [Fact]
    public void Script_SortsByAtStably_AndLeavesOutADirectiveThatWaitsForAnEvent()
    {
        using var script = CaseScript.Parse(JsonDocument.Parse("""
            { "script": [
              { "at": 2000, "channel": "trading", "messageType": "trading", "value": [] },
              { "after": { "event": "snapshotServed", "batch": 1 }, "directive": "connectionDrop", "channels": ["trading"] },
              { "at": 1000, "directive": "heartbeatStop", "channels": ["trading"] },
              { "at": 1000, "channel": "incident", "messageType": "incident", "value": [] }
            ] }
            """));

        int[] order = [2, 3, 0];
        Assert.Equal(order, script.Timeline.Select(entry => entry.Index));
        Assert.True(script.Timeline[0].IsDirective);
    }

    [Fact]
    public void Rewrite_GivesARecordedMatchOneNewGuid_OnEveryChannel_SystemEventsAndRemovedSubscriptionsIncluded()
    {
        var (script, rewriter) = Rewriter(1);
        using (script)
        {
            var messages = script.Timeline.Select(rewriter.Rewrite).ToArray();
            var newId = rewriter.MatchIds["m1"];

            Assert.True(Guid.TryParse(newId, out var guid) && guid.ToString("D") == newId);
            Assert.All(messages.Take(8), message => Assert.All(Ids(message.Payload), id => Assert.Equal(newId, id)));
            var events = Assert.IsType<GlobalMessageModel[]>(messages[8].Payload);
            Assert.Equal(HubMethods.OnSystemEvent, messages[8].Method);
            Assert.Equal(newId, events[0].Id);
            Assert.Equal(rewriter.MatchIds["m2"], events[1].Id);
            var removed = Assert.IsType<string[]>(messages[9].Payload);
            Assert.Equal(HubMethods.OnSubscriptionsRemove, messages[9].Method);
            Assert.Equal(new[] { newId, rewriter.MatchIds["m3"] }, removed);
            Assert.Equal(3, rewriter.MatchIds.Values.Distinct().Count());
        }
    }

    [Fact]
    public void Rewrite_OfTwoRuns_NeverSharesAMatchId()
    {
        var (first, one) = Rewriter(1);
        var (second, other) = Rewriter(1);
        using (first)
        using (second)
        {
            one.Rewrite(first.Timeline[0]);
            other.Rewrite(second.Timeline[0]);

            Assert.NotEqual(one.MatchIds["m1"], other.MatchIds["m1"]);
        }
    }

    [Fact]
    public void Rewrite_NumbersAnUpdateWithTheNextOffset_AndRepeatsTheKeptOneForAnyOtherMsgType()
    {
        var offsets = new OffsetSequence();
        var (script, rewriter) = Rewriter(1, offsets);
        using (script)
        {
            var timeTable = (TimeTableItemModel[])rewriter.Rewrite(script.Timeline[0]).Payload;
            var trading = (TradingInfoModel[])rewriter.Rewrite(script.Timeline[1]).Payload;
            var timeTableAgain = (TimeTableItemModel[])rewriter.Rewrite(script.Timeline[4]).Payload;
            var tradingAgain = (TradingInfoModel[])rewriter.Rewrite(script.Timeline[5]).Payload;
            var timeTableLast = (TimeTableItemModel[])rewriter.Rewrite(script.Timeline[10]).Payload;

            Assert.Equal(1, timeTable[0].Offset);
            Assert.Equal(2, trading[0].Offset);
            Assert.Equal(1, timeTableAgain[0].Offset);
            Assert.Equal(3, tradingAgain[0].Offset);
            Assert.Equal(3, tradingAgain[1].Offset);
            Assert.Equal(4, timeTableLast[0].Offset);
        }
    }

    [Fact]
    public void Rewrite_TakesTheNextOffsetForANonUpdateWithNoKeptOffset_FromTheCounterSharedByEveryRun()
    {
        var offsets = new OffsetSequence();
        var (first, one) = Rewriter(1, offsets);
        var (second, other) = Rewriter(1, offsets);
        using (first)
        using (second)
        {
            var snapshot = Item<TimeTableItemModel>(one.Rewrite(first.Timeline[4]));
            var update = Item<TimeTableItemModel>(other.Rewrite(second.Timeline[0]));
            var again = Item<TimeTableItemModel>(one.Rewrite(first.Timeline[0]));

            Assert.Equal(1, snapshot.Offset);
            Assert.Equal(2, update.Offset);
            Assert.Equal(3, again.Offset);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    public void Rewrite_MovesEveryTimestampOntoTheRunClock_ScaledByTheAcceleration(double acceleration)
    {
        var (script, rewriter) = Rewriter(acceleration);
        using (script)
        {
            var started = StartedAt.ToUnixTimeMilliseconds();
            long Shifted(long recorded, long start) => started + (long)Math.Round((recorded - start) / acceleration);
            var messages = script.Timeline.Select(rewriter.Rewrite).ToArray();

            var timeTable = Item<TimeTableItemModel>(messages[0]);
            Assert.Equal(started, timeTable.Timestamp);
            Assert.Equal(Shifted(TimeTableBase + 3_600_000, TimeTableBase), new DateTimeOffset(timeTable.StartDate!.Value).ToUnixTimeMilliseconds());
            Assert.Equal(DateTimeKind.Utc, timeTable.StartDate.Value.Kind);
            var laterTimeTable = Item<TimeTableItemModel>(messages[4]);
            Assert.Equal(Shifted(TimeTableBase + 2000, TimeTableBase), laterTimeTable.Timestamp);
            Assert.Equal(Shifted(TimeTableBase + 3_602_000, TimeTableBase), new DateTimeOffset(laterTimeTable.StartDate!.Value).ToUnixTimeMilliseconds());
            Assert.Null(Item<TimeTableItemModel>(messages[10]).StartDate);

            Assert.Equal(started, Item<TradingInfoModel>(messages[1]).Timestamp);
            var trading = (TradingInfoModel[])messages[5].Payload;
            Assert.Equal(Shifted(TradingBase + 4000, TradingBase), trading[0].Timestamp);
            Assert.Equal(Shifted(TradingBase + 4100, TradingBase), trading[1].Timestamp);

            var scoreboard = Item<ScoreBoardModel>(messages[2]);
            Assert.Equal(started, scoreboard.Timestamp);
            Assert.Equal(started, scoreboard.Timer.TimeStamp);
            var laterScoreboard = Item<ScoreBoardModel>(messages[6]);
            Assert.Equal(Shifted(ScoreboardBase + 2000, ScoreboardBase), laterScoreboard.Timestamp);
            Assert.Null(laterScoreboard.Timer);

            Assert.Equal(StartedAt.UtcDateTime, Item<IncidentModel>(messages[3]).Date);
            Assert.Equal(DateTimeKind.Utc, Item<IncidentModel>(messages[3]).Date.Kind);
            Assert.Equal(Shifted(TimeTableBase + 2900, TimeTableBase + 900), new DateTimeOffset(Item<IncidentModel>(messages[7]).Date).ToUnixTimeMilliseconds());
        }
    }

    [Fact]
    public void Rewrite_LeavesEveryOtherFieldAsRecorded()
    {
        var (script, rewriter) = Rewriter(20);
        using (script)
        {
            var messages = script.Timeline.Select(rewriter.Rewrite).ToArray();

            var timeTable = Item<TimeTableItemModel>(messages[0]);
            Assert.Equal(6, timeTable.SportId);
            Assert.Equal(1, timeTable.Status);
            Assert.Equal("A vs B", timeTable.Name["en"]);
            Assert.Equal(1, timeTable.MsgType);
            var trading = Item<TradingInfoModel>(messages[1]);
            var outcome = trading.Markets.Single().Outcomes.Single();
            Assert.Equal("mk", trading.Markets.Single().Id);
            Assert.Equal(1.5m, outcome.Price);
            Assert.Equal(1.53, outcome.Prices["decimal"]);
            Assert.Equal("-188", outcome.Prices["american"]);
            var scoreboard = Item<ScoreBoardModel>(messages[2]);
            Assert.Equal(2, scoreboard.Stage);
            Assert.Equal(300, scoreboard.Timer.TimerValue);
            Assert.True(scoreboard.Timer.Enabled);
            var incident = Item<IncidentModel>(messages[3]);
            Assert.Equal(3, incident.Index);
            Assert.Equal("1029", incident.Type);
            var events = (GlobalMessageModel[])messages[8].Payload;
            Assert.Equal(GlobalEventType.BetStateChange, events[0].EventType);
            Assert.Equal(GlobalEventValue.MatchBetStop, events[0].EventValue);
        }
    }

    [Theory]
    [InlineData("""{ "at": 0, "channel": "trading", "messageType": "odds", "value": [] }""", "unsupported messageType 'odds'")]
    [InlineData("""{ "at": 0, "channel": "trading", "messageType": "scoreboard", "value": [] }""", "messageType 'scoreboard' is not sent on channel 'trading'")]
    [InlineData("""{ "at": 0, "channel": "trading", "messageType": "trading", "value": { "id": "m1" } }""", "the value does not deserialize as trading")]
    [InlineData("""{ "at": 0, "channel": "trading", "messageType": "trading", "value": null }""", "the value of a trading entry is null")]
    [InlineData("""{ "at": 0, "channel": "trading", "messageType": "trading", "value": [ { "timestamp": 1, "msgType": 1 } ] }""", "item 0 has no id")]
    [InlineData("""{ "at": 0, "channel": "time_table", "messageType": "time_table", "value": [ { "id": "m1", "timestamp": 1 }, { "id": "m1", "msgType": 1 } ] }""", "item 1 has no timestamp")]
    [InlineData("""{ "at": 0, "channel": "scoreboard", "messageType": "scoreboard", "value": [ { "id": "m1" } ] }""", "item 0 has no timestamp")]
    [InlineData("""{ "at": 0, "channel": "incident", "messageType": "incident", "value": [ { "id": "m1", "timestamp": 1 } ] }""", "item 0 has no date")]
    public void Rewrite_OfAnEntryThatCannotBePlayed_ThrowsNamingTheCause(string entry, string cause)
    {
        using var script = CaseScript.Parse(JsonDocument.Parse($$"""{ "script": [ {{entry}} ] }"""));
        var rewriter = new PlaybackRewriter(StartedAt, 1, new OffsetSequence(), script.Bases);

        var exception = Assert.Throws<InvalidDataException>(() => rewriter.Rewrite(script.Timeline[0]));

        Assert.StartsWith(cause, exception.Message, StringComparison.Ordinal);
    }

    private static (CaseScript script, PlaybackRewriter rewriter) Rewriter(double acceleration, OffsetSequence offsets = null)
    {
        var script = CaseScript.Parse(JsonDocument.Parse(Script));
        return (script, new PlaybackRewriter(StartedAt, acceleration, offsets ?? new OffsetSequence(), script.Bases));
    }

    private static T Item<T>(PlaybackMessage message) => Assert.Single((T[])message.Payload);

    private static IEnumerable<string> Ids(object payload) => payload switch
    {
        TimeTableItemModel[] items => items.Select(item => item.Id),
        TradingInfoModel[] items => items.Select(item => item.Id),
        ScoreBoardModel[] items => items.Select(item => item.Id),
        IncidentModel[] items => items.Select(item => item.Id),
        _ => throw new InvalidOperationException(payload.GetType().Name),
    };
}
