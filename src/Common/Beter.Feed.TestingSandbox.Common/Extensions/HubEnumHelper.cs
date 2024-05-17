using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Enums;

namespace Beter.Feed.TestingSandbox.Common.Extensions;

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
