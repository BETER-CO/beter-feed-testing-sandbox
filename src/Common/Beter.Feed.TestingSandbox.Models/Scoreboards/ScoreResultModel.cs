using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.Scoreboards;

[MessagePackObject]
public class ScoreResultModel
{
    public ScoreResultModel()
    {

    }
    [Key("result")] public int Result { get; set; }
    [Key("value")] public int Value { get; set; }
}
