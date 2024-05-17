using Beter.Feed.TestingSandbox.Models;
using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.Incidents;

[MessagePackObject]
public class IncidentModel : IIncident, IFeedMessage
{
    public IncidentModel()
    {

    }
    [Key("id")] public string Id { get; set; }
    [Key("sportId")] public int? SportId { get; set; }
    [Key("index")] public int Index { get; set; }
    [Key("type")] public string Type { get; set; }
    [Key("date")] public DateTime Date { get; set; }
    [Key("params")] public IncidentParameterModel[] Params { get; set; }
    [Key("messageType")] public int MsgType { get; set; }
    [Key("offset")] public long Offset { get; set; }
}
