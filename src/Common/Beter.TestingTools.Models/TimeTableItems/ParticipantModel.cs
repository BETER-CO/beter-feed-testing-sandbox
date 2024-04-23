using MessagePack;

namespace Beter.TestingTools.Models.TimeTableItems;

[MessagePackObject]
public class ParticipantModel : NamedIdentityModelBase
{
    public ParticipantModel()
    {

    }
    [Key("team")] public TeamModel Team { get; set; }
    [Key("color")] public string Color { get; set; }
}
