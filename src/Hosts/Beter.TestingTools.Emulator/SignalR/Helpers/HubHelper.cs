using Beter.TestingTools.Common.Enums;
using Beter.TestingTools.Models.Incidents;
using Beter.TestingTools.Models.Scoreboards;
using Beter.TestingTools.Models.TimeTableItems;
using Beter.TestingTools.Models.TradingInfos;

namespace Beter.TestingTools.Emulator.SignalR.Helpers;

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
