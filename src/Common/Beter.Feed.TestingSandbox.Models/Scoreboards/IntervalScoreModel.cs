using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.Scoreboards;

[MessagePackObject]
public class IntervalScoreModel
{
    public IntervalScoreModel()
    {

    }
    [Key("interval")] public int? Interval { get; set; }
    [Key("result")] public int? Result { get; set; }
    [Key("score")] public string Score { get; set; }
    [Key("subInterval")] public int? SubInterval { get; set; }
}
