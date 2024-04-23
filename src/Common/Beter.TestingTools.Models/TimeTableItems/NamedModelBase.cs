using MessagePack;

namespace Beter.TestingTools.Models.TimeTableItems;

[MessagePackObject]
public class NamedModelBase
{
    public NamedModelBase()
    {

    }
    [Key("name")] public Dictionary<string, string> Name { get; set; }
}


