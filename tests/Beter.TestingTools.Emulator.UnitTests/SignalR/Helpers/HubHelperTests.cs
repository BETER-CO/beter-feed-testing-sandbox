using Beter.TestingTools.Common.Enums;
using Beter.TestingTools.Emulator.SignalR.Helpers;
using Beter.TestingTools.Models.Incidents;
using Beter.TestingTools.Models.Scoreboards;
using Beter.TestingTools.Models.TimeTableItems;
using Beter.TestingTools.Models.TradingInfos;

namespace Beter.TestingTools.Emulator.UnitTests.SignalR.Helpers
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
