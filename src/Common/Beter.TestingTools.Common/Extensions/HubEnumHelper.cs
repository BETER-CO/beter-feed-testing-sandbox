using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Common.Enums;

namespace Beter.TestingTools.Common.Extensions;

public static class HubEnumHelper
{
    public static HubKind ToHub(string channel) => channel switch
    {
        ChannelNames.Timetable => HubKind.TimeTable,
        ChannelNames.Scoreboard => HubKind.Scoreboard,
        ChannelNames.Trading => HubKind.Trading,
        ChannelNames.Incident => HubKind.Incident,
        _ => HubKind.Undefined,
    };
}
