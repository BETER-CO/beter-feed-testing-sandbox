using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.Scoreboards;

[MessagePackObject]
public class PlayerPropsModel
{
    public PlayerPropsModel()
    {

    }
    [Key("participantId")] public string ParticipantId { get; set; }
    [Key("interval")] public int Interval { get; set; }
    [Key("results")] public IEnumerable<ScoreResultModel> Results { get; set; }
}
