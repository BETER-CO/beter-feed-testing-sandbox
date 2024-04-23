using MessagePack;

namespace Beter.TestingTools.Models.GlobalEvents;

[MessagePackObject]
public class GlobalMessageModel : IGlobalMessageModel
{
    [Key("Id")] public string Id { get; set; }
    [Key("eventType")] public GlobalEventType EventType { get; set; }
    [Key("eventValue")] public GlobalEventValue EventValue { get; set; }
}