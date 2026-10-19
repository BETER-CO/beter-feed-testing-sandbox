using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.Scoreboards;

[MessagePackObject]
public class WinnerScoreModel
{
    public WinnerScoreModel()
    {
    }

    [Key("interval")] public int Interval { get; set; }
    [Key("subInterval")] public int? SubInterval { get; set; }
    [Key("result")] public int Result { get; set; }
    [Key("winner")] public int Winner { get; set; }
}
