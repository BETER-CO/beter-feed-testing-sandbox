using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.TimeTableItems;

[MessagePackObject]
public class ExcludedParticipantModel
{
    public ExcludedParticipantModel()
    {

    }
    [Key("parentParticipantId")] public string ParentParticipantId { get; set; }
    [Key("order")] public int Order { get; set; }
    [Key("id")] public string Id { get; set; }
    [Key("participantType")] public int ParticipantType { get; set; }
    [Key("name")] public string Name { get; set; }
    [Key("attributes")] public Dictionary<string, string> Attributes { get; set; }
    [Key("composition")] public List<ExcludedParticipantModel> Composition { get; set; } = new();
}
