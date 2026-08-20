using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.TimeTableItems;

[MessagePackObject]
public sealed class ParticipantStructureModel
{
    public ParticipantStructureModel()
    {

    }
    [Key("id")] public string Id { get; set; }
    [Key("participantType")] public int ParticipantType { get; set; }
    [Key("order")] public int Order { get; set; }
    [Key("name")] public string Name { get; set; }
    [Key("attributes")] public Dictionary<string, string> Attributes { get; set; }
    [Key("composition")] public ParticipantStructureModel[] Composition { get; set; } = Array.Empty<ParticipantStructureModel>();
}
