using AutoFixture;
using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Generator.Application.Contracts.Playbacks;
using Beter.TestingTools.Generator.Application.Extensions;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations;
using Beter.TestingTools.Models.Incidents;
using Beter.TestingTools.Models.TradingInfos;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.Playbacks.Transformations.TransformationExtTests
{
    public class UpdateTimestampAndDateTests
    {
        private readonly Fixture _fixture = new();

        [Fact]
        public void UpdateTimestampAndDate_Updates_Timestamp_And_Date_When_ReplyMode_Is_HistoricalTimeline()
        {
            // Arrange
            var originalDate = DateTime.SpecifyKind(new DateTime(2024, 4, 10), DateTimeKind.Utc);
            var model = _fixture.Build<IncidentModel>()
              .With(x => x.Id, _fixture.Create<string>())
              .With(x => x.Date, originalDate)
              .Create();

            var oldFirstMessageTimestamp = DateTime.SpecifyKind(new DateTime(2024, 4, 1), DateTimeKind.Utc);
            var profile = _fixture.Build<MessagesTransformationContext.MatchIdProfile>()
                .With(x => x.OldFirstTimestampByEachMessageType, new Dictionary<string, DateTime>
                {
                    { MessageTypes.Incident, oldFirstMessageTimestamp },
                    { MessageTypes.Trading, DateTime.UtcNow }
                })
                .Create();

            var testStartDate = DateTime.SpecifyKind(new DateTime(2024, 4, 20), DateTimeKind.Utc);
            var context = _fixture.Build<MessagesTransformationContext>()
                .With(x => x.TestCaseStart, testStartDate)
                .With(x => x.ReplyMode, ReplyMode.HistoricalTimeline)
                .With(x => x.Matches, new Dictionary<string, MessagesTransformationContext.MatchIdProfile> { { model.Id, profile } })
                .Create();

            var expected = DateTime.SpecifyKind(new DateTime(2024, 4, 29), DateTimeKind.Utc);

            // Act
            TransformationsExt.UpdateTimestampAndDate(
                model,
                context,
                model => model.Date,
                (model, dateTime) => model.Date = dateTime);

            // Assert
            Assert.Equal(expected, model.Date);
        }

        [Fact]
        public void UpdateTimestampAndDate_Throws_NotImplementedException_When_ReplyMode_Is_Not_HistoricalTimeline()
        {
            // Arrange
            var model = _fixture.Create<IncidentModel>();
            var context = _fixture.Build<MessagesTransformationContext>()
                .With(x => x.ReplyMode, ReplyMode.FixedDelay)
                .Create();

            // Act & Assert
            Assert.Throws<NotImplementedException>(
                () => TransformationsExt.UpdateTimestampAndDate(
                    model,
                    context,
                    model => model.Date,
                    (model, dateTime) => model.Date = dateTime));
        }

        [Fact]
        public void UpdateTimestampAndDate_Updates_Timestamp_And_Date_When_ReplyMode_Is_HistoricalTimeline2()
        {
            // Arrange
            var originalDate = DateTime.SpecifyKind(new DateTime(2024, 4, 10), DateTimeKind.Utc).ToUnixTimeMilliseconds();
            var model = _fixture.Build<TradingInfoModel>()
              .With(x => x.Id, _fixture.Create<string>())
              .With(x => x.Timestamp, originalDate)
              .Create();

            var oldFirstMessageTimestamp = DateTime.SpecifyKind(new DateTime(2024, 4, 1), DateTimeKind.Utc);
            var profile = _fixture.Build<MessagesTransformationContext.MatchIdProfile>()
                .With(x => x.OldFirstTimestampByEachMessageType, new Dictionary<string, DateTime>
                {
                    { MessageTypes.Incident, DateTime.UtcNow },
                    { MessageTypes.Trading, oldFirstMessageTimestamp }
                })
                .Create();

            var testStartDate = DateTime.SpecifyKind(new DateTime(2024, 4, 20), DateTimeKind.Utc);
            var context = _fixture.Build<MessagesTransformationContext>()
                .With(x => x.TestCaseStart, testStartDate)
                .With(x => x.ReplyMode, ReplyMode.HistoricalTimeline)
                .With(x => x.Matches, new Dictionary<string, MessagesTransformationContext.MatchIdProfile> { { model.Id, profile } })
                .Create();

            var expected = DateTime.SpecifyKind(new DateTime(2024, 4, 29), DateTimeKind.Utc).ToUnixTimeMilliseconds();

            // Act
            TransformationsExt.UpdateTimestampAndDate(
                model,
                context,
                model => model.Timestamp,
                (model, timestamp) => model.Timestamp = timestamp);

            // Assert
            Assert.Equal(expected, model.Timestamp);
        }

        [Fact]
        public void UpdateTimestampAndDate_Throws_NotImplementedException_When_ReplyMode_Is_Not_HistoricalTimeline2()
        {
            // Arrange
            var model = _fixture.Create<TradingInfoModel>();
            var context = _fixture.Build<MessagesTransformationContext>()
                .With(x => x.ReplyMode, ReplyMode.FixedDelay)
                .Create();

            // Act & Assert
            Assert.Throws<NotImplementedException>(
                () => TransformationsExt.UpdateTimestampAndDate(
                    model,
                    context,
                    model => model.Timestamp,
                    (model, timestamp) => model.Timestamp = timestamp));
        }
    }
}
