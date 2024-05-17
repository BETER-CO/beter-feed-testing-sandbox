using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Emulator.SignalR.Helpers;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Models.TradingInfos;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.SignalR.Helpers
{
    public class HubHelperTests
    {
        [Fact]
        public void ToHubScoreboard_ReturnsCorrectHubKind()
        {
            // Act
            var actual = HubHelper.ToHub<ScoreBoardModel>();

            // Assert
            Assert.Equal(HubKind.Scoreboard, actual);
        }

        [Fact]
        public void ToHubTradingInfo_ReturnsCorrectHubKind()
        {
            // Act
            var actual = HubHelper.ToHub<TradingInfoModel>();

            // Assert
            Assert.Equal(HubKind.Trading, actual);
        }

        [Fact]
        public void ToHubTimeTableItem_ReturnsCorrectHubKind()
        {
            // Act
            var actual = HubHelper.ToHub<TimeTableItemModel>();

            // Assert
            Assert.Equal(HubKind.TimeTable, actual);
        }

        [Fact]
        public void ToHubIncident_ReturnsCorrectHubKind()
        {
            // Act
            var actual = HubHelper.ToHub<IncidentModel>();

            // Assert
            Assert.Equal(HubKind.Incident, actual);
        }
    }
}
