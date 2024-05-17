using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Models.TradingInfos;

namespace Beter.Feed.TestingSandbox.Emulator.SignalR.Helpers;

public static class HubHelper
{
    public static HubKind ToHub<T>() => typeof(T) switch
    {
        Type intType when intType == typeof(TimeTableItemModel) => HubKind.TimeTable,
        Type intType when intType == typeof(ScoreBoardModel) => HubKind.Scoreboard,
        Type intType when intType == typeof(TradingInfoModel) => HubKind.Trading,
        Type intType when intType == typeof(IncidentModel) => HubKind.Incident,
        _ => HubKind.Undefined,
    };
}
