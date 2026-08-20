using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations.Rules;
using Beter.Feed.TestingSandbox.Generator.Application.Services;
using Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Repositories;
using Beter.Feed.TestingSandbox.Generator.Infrastructure.Services;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.TestScenarios
{
    /// <summary>
    /// Guards the CS2 scenario that exercises playerProps, participantStructure and
    /// excludedParticipants. These run the real playback pipeline rather than a hand-rolled
    /// serialize, so they fail if any of those fields stop surviving a playback.
    /// </summary>
    public class CounterStrikeScenarioTests
    {
        private const int CounterStrikeCaseId = 3001;

        private static readonly TestScenario Scenario = LoadScenario();

        private static TestScenario LoadScenario()
        {
            var scenarios = new TestScenarioFactory().Create(TestScenario.ResourcesPath);

            return scenarios.Single(scenario => scenario.CaseId == CounterStrikeCaseId);
        }

        [Fact]
        public void Scenario_IsLoaded_WithAllFourChannels()
        {
            // Assert
            Assert.Equal("1.0", Scenario.Version.ToString());
            Assert.NotEmpty(Scenario.Messages);

            var channels = Scenario.Messages.Select(message => message.MessageType).Distinct();

            Assert.Contains(MessageTypes.Timetable, channels);
            Assert.Contains(MessageTypes.Scoreboard, channels);
            Assert.Contains(MessageTypes.Trading, channels);
            Assert.Contains(MessageTypes.Incident, channels);
        }

        [Fact]
        public void Scenario_OpensWithBookingSnapshot_AndNeverDeclaresAConnectionSnapshot()
        {
            // The client is already connected in this scenario, so nothing is delivered as a
            // connection snapshot. Declaring one would also make the generator publish an empty
            // terminating message on that channel, which does not belong to this playback.
            var timeTables = TimeTableItems(Scenario).ToList();
            var scoreBoards = ScoreBoards(Scenario).ToList();

            Assert.Equal((int)MessageType.BookingSnapshot, timeTables.First().MsgType);
            Assert.Equal((int)MessageType.BookingSnapshot, scoreBoards.First().MsgType);

            Assert.All(timeTables.Skip(1), item => Assert.Equal((int)MessageType.Update, item.MsgType));
            Assert.All(scoreBoards.Skip(1), item => Assert.Equal((int)MessageType.Update, item.MsgType));

            Assert.DoesNotContain((int)MessageType.ConnectionSnapshot, timeTables.Select(x => x.MsgType));
            Assert.DoesNotContain((int)MessageType.ConnectionSnapshot, scoreBoards.Select(x => x.MsgType));
        }

        [Fact]
        public void BookingSnapshot_IsDeliveredOnceAcrossChannels_AtTheSameMoment()
        {
            // A match is booked once. The booking snapshot goes out across every channel at that
            // moment, so it must never appear twice for the same match at different times.
            var bookingSnapshots = Scenario.Messages
                .Where(message => message.ToFeedMessages().Any(feed => feed.MsgType == (int)MessageType.BookingSnapshot))
                .ToList();

            Assert.Equal(2, bookingSnapshots.Count);

            var channels = bookingSnapshots.Select(message => message.MessageType).ToList();

            Assert.Equal(channels.Count, channels.Distinct().Count());

            var spanInSeconds = (bookingSnapshots.Max(message => message.ScheduledAt)
                               - bookingSnapshots.Min(message => message.ScheduledAt)) / 1000d;

            Assert.True(spanInSeconds <= 1, $"booking snapshots span {spanInSeconds:0.##}s, expected them within one second");
        }

        [Fact]
        public void MatchSnapshot_FollowsTheBookingSnapshot_OnTheTradingChannel()
        {
            // The match snapshot marks trading going live, so it comes after booking, not between
            // the booking snapshot messages.
            var lastBookingSnapshot = Scenario.Messages
                .Where(message => message.ToFeedMessages().Any(feed => feed.MsgType == (int)MessageType.BookingSnapshot))
                .Max(message => message.ScheduledAt);

            var matchSnapshot = Scenario.Messages
                .Single(message => message.ToFeedMessages().Any(feed => feed.MsgType == (int)MessageType.MatchSnapshot));

            Assert.Equal(MessageTypes.Trading, matchSnapshot.MessageType);
            Assert.True(matchSnapshot.ScheduledAt > lastBookingSnapshot,
                "the match snapshot must be scheduled after the booking snapshot");
        }

        [Fact]
        public void TimeTable_ExcludesAPlayer_AndAddsAReplacement()
        {
            // Arrange
            var timeTables = TimeTableItems(Scenario).ToList();

            // Assert - the roster starts complete, then one player is excluded
            Assert.Empty(timeTables.First().ExcludedParticipants);

            var afterExclusion = timeTables.Last();

            Assert.Single(afterExclusion.ExcludedParticipants);

            var excluded = afterExclusion.ExcludedParticipants.Single();
            var parent = afterExclusion.ParticipantStructure.Single(team => team.Id == excluded.ParentParticipantId);

            Assert.DoesNotContain(parent.Composition, player => player.Id == excluded.Id);
            Assert.Equal(timeTables.First().ParticipantStructure.Sum(team => team.Composition.Length),
                         afterExclusion.ParticipantStructure.Sum(team => team.Composition.Length));
        }

        [Fact]
        public void PlayerProps_AlwaysResolveIntoTheRosterOrTheExcludedList()
        {
            // Arrange - the roster as of the last timetable message, which is what props resolve against
            var lastTimeTable = TimeTableItems(Scenario).Last();

            var known = lastTimeTable.ParticipantStructure
                .SelectMany(team => team.Composition)
                .Select(player => player.Id)
                .Concat(lastTimeTable.ParticipantStructure.Select(team => team.Id))
                .Concat(lastTimeTable.ExcludedParticipants.Select(excluded => excluded.Id))
                .ToHashSet();

            var referenced = ScoreBoards(Scenario)
                .Where(scoreboard => scoreboard.PlayerProps != null)
                .SelectMany(scoreboard => scoreboard.PlayerProps)
                .Select(props => props.ParticipantId)
                .Distinct()
                .ToList();

            // Assert
            Assert.NotEmpty(referenced);
            Assert.All(referenced, participantId => Assert.Contains(participantId, known));
        }

        [Fact]
        public void ExcludedPlayerKeepsPlayerProps_AndTheReplacementGainsThem()
        {
            // Arrange
            var lastTimeTable = TimeTableItems(Scenario).Last();
            var excludedId = lastTimeTable.ExcludedParticipants.Single().Id;

            var finalProps = ScoreBoards(Scenario)
                .Last(scoreboard => scoreboard.PlayerProps != null && scoreboard.PlayerProps.Any())
                .PlayerProps
                .Select(props => props.ParticipantId)
                .ToHashSet();

            var replacementId = lastTimeTable.ParticipantStructure
                .SelectMany(team => team.Composition)
                .Select(player => player.Id)
                .Except(TimeTableItems(Scenario).First().ParticipantStructure.SelectMany(team => team.Composition).Select(player => player.Id))
                .Single();

            // Assert
            Assert.Contains(excludedId, finalProps);
            Assert.Contains(replacementId, finalProps);
        }

        [Fact]
        public void Playback_RewritesTheMatchId_ButLeavesParticipantIdsAlone()
        {
            // Arrange
            var originalMatchId = TimeTableItems(Scenario).First().Id;
            var originalPlayerIds = TimeTableItems(Scenario).First()
                .ParticipantStructure.SelectMany(team => team.Composition)
                .Select(player => player.Id)
                .ToList();

            // Act
            var playback = CreatePlaybackFactory().Create(
                CounterStrikeCaseId,
                ReplyMode.HistoricalTimeline,
                timeOffsetInMinutes: 0,
                timeOffsetBetweenMessagesInSecounds: null,
                timeOffsetAfterFirstTimetableMessageInSecounds: 0,
                accelerationFactor: 1);

            var played = playback.Messages.Values
                .Select(item => item.Message)
                .Where(message => message.IsMessageType(MessageTypes.Timetable))
                .SelectMany(message => message.GetValue<IEnumerable<TimeTableItemModel>>())
                .ToList();

            // Assert - match id is regenerated per playback
            Assert.All(played, item => Assert.NotEqual(originalMatchId, item.Id));

            // Assert - participant ids are stable, which is what keeps playerProps resolvable
            var playedPlayerIds = played
                .SelectMany(item => item.ParticipantStructure)
                .SelectMany(team => team.Composition)
                .Select(player => player.Id)
                .Distinct();

            Assert.All(originalPlayerIds, playerId => Assert.Contains(playerId, playedPlayerIds));
        }

        [Fact]
        public void Playback_PreservesPlayerProps_WithTheirValues()
        {
            // Arrange - the same participant and interval recurs across messages as stats advance,
            // so compare the whole set of entries rather than keying on participant and interval.
            var expected = ScoreBoards(Scenario).SelectMany(Describe).OrderBy(entry => entry).ToList();

            // Act
            var playback = CreatePlaybackFactory().Create(
                CounterStrikeCaseId,
                ReplyMode.HistoricalTimeline,
                timeOffsetInMinutes: 0,
                timeOffsetBetweenMessagesInSecounds: null,
                timeOffsetAfterFirstTimetableMessageInSecounds: 0,
                accelerationFactor: 1);

            var played = playback.Messages.Values
                .Select(item => item.Message)
                .Where(message => message.IsMessageType(MessageTypes.Scoreboard))
                .SelectMany(message => message.GetValue<IEnumerable<ScoreBoardModel>>())
                .ToList();

            // Assert
            Assert.NotEmpty(played);
            Assert.All(played, scoreboard => Assert.NotEmpty(scoreboard.PlayerProps));

            var actual = played.SelectMany(Describe).OrderBy(entry => entry).ToList();

            Assert.Equal(expected, actual);
        }

        /// <summary>Flattens playerProps into comparable text so values, not just presence, are asserted.</summary>
        private static IEnumerable<string> Describe(ScoreBoardModel scoreboard) =>
            scoreboard.PlayerProps.Select(props =>
                $"{props.ParticipantId}|{props.Interval}|" +
                string.Join(",", props.Results.Select(result => $"{result.Result}={result.Value}")));

        private static IEnumerable<TimeTableItemModel> TimeTableItems(TestScenario scenario) =>
            scenario.Messages
                .Where(message => message.IsMessageType(MessageTypes.Timetable))
                .SelectMany(message => message.GetValue<IEnumerable<TimeTableItemModel>>());

        private static IEnumerable<ScoreBoardModel> ScoreBoards(TestScenario scenario) =>
            scenario.Messages
                .Where(message => message.IsMessageType(MessageTypes.Scoreboard))
                .SelectMany(message => message.GetValue<IEnumerable<ScoreBoardModel>>());

        private static PlaybackFactory CreatePlaybackFactory()
        {
            var systemClock = new SystemClock();
            var repository = new InMemoryTestScenariosRepository(new[] { Scenario });

            var contextFactory = new MessagesTransformationContextFactory(
                systemClock,
                new RunCountTracker(),
                new MatchIdGenerator());

            var manager = new TransformationManager(new ITransformationRule[]
            {
                new IncidentTransformationRule(),
                new TradingTransformationRule(),
                new ScoreboardTransformationRule(),
                new TimeTableTransformationRule(),
            });

            return new PlaybackFactory(systemClock, repository, manager, contextFactory);
        }
    }
}
