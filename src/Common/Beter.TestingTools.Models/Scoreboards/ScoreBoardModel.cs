using MessagePack;

namespace Beter.TestingTools.Models.Scoreboards;

[MessagePackObject]
public class ScoreBoardModel : IFeedMessage
{
    public ScoreBoardModel()
    {

    }
    [Key("id")] public string Id { get; set; }
    [Key("sportId")] public int? SportId { get; set; }
    [Key("stage")] public int? Stage { get; set; }
    [Key("scores")] public IEnumerable<IntervalScoreModel> Scores { get; set; }
    [Key("timer")] public TimerModel Timer { get; set; }
    [Key("firstServer")] public string FirstServer { get; set; }
    [Key("server")] public string Server { get; set; }
    [Key("comment")] public Dictionary<string, string> Comment { get; set; }
    [Key("regulations")] public RegulationsModel Regulations { get; set; }
    [Key("timestamp")] public long Timestamp { get; set; }
    [Key("messageType")] public int MsgType { get; set; }
    [Key("offset")] public long Offset { get; set; }
}
